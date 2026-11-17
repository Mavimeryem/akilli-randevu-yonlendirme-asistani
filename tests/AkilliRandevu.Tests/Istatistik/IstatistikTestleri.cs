using System.Text;
using AkilliRandevu.Business.Istatistik;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Concrete;
using AkilliRandevu.Entities.Enums;
using AkilliRandevu.Tests.Yardimcilar;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Tests.Istatistik;

/// <summary>Hafta 12: metrik hesapları, eksik veri, sıfıra bölme, yoğunluk, sıra panosu ve CSV dışa aktarma.</summary>
public class IstatistikTestleri : IDisposable
{
    private readonly TestVeritabani _vt = new();

    public void Dispose() => _vt.Dispose();

    private static readonly TakvimAyarlari Ayar = new(); // 09-17, öğle 12-13, 20 dk → günde 21 slot
    private static readonly List<DoktorOzeti> TekDoktor = [new(1, "Uzm. Dr. Test Doktor", 1, "Dahiliye")];

    private static RandevuOzeti R(int id, DateTime bas, RandevuDurumu durum, int doktorId = 1, int bolumId = 1, string ad = "Deniz", string soyad = "Acar") =>
        new(id, bas, bas.AddMinutes(20), durum, doktorId, "Uzm. Dr. Test Doktor", bolumId, "Dahiliye", ad, soyad);

    // ---------------- Hesap mantığı ----------------

    [Fact]
    public void Doluluk_aktif_randevu_bolu_kapasitedir_iptal_sayilmaz()
    {
        var pazartesi = new DateOnly(2026, 11, 16);
        var randevular = new List<RandevuOzeti>
        {
            R(1, new DateTime(2026, 11, 16, 9, 0, 0), RandevuDurumu.Tamamlandi),
            R(2, new DateTime(2026, 11, 16, 9, 20, 0), RandevuDurumu.Gelmedi),
            R(3, new DateTime(2026, 11, 16, 9, 40, 0), RandevuDurumu.IptalEdildi)
        };

        var r = IstatistikHesaplayici.Hesapla(randevular, TekDoktor, pazartesi, pazartesi, Ayar, new DateTime(2026, 11, 17));

        Assert.Equal(21, r.Kapasite);
        Assert.Equal(3, r.Toplam);
        Assert.Equal(2, r.Aktif);
        Assert.Equal(2.0 / 21, r.Doluluk);
        Assert.Equal(1.0 / 3, r.IptalOrani);
        Assert.Equal(0.5, r.GelmemeOrani);
    }

    [Fact]
    public void Sonucu_girilmemis_gecmis_randevular_gelmeme_oranina_katilmaz_ve_ayrica_sayilir()
    {
        var gun = new DateOnly(2026, 11, 16);
        var randevular = new List<RandevuOzeti>
        {
            R(1, new DateTime(2026, 11, 16, 9, 0, 0), RandevuDurumu.Tamamlandi),
            R(2, new DateTime(2026, 11, 16, 9, 20, 0), RandevuDurumu.Onaylandi),   // saati geçti, sonuç yok
            R(3, new DateTime(2026, 11, 16, 15, 0, 0), RandevuDurumu.Beklemede)    // henüz saati gelmedi
        };

        var r = IstatistikHesaplayici.Hesapla(randevular, TekDoktor, gun, gun, Ayar, new DateTime(2026, 11, 16, 12, 0, 0));

        Assert.Equal(1, r.Sonuclanmamis);
        Assert.Equal(0.0, r.GelmemeOrani);   // 0 gelmedi / 1 tamamlanan
    }

    [Fact]
    public void Payda_sifirsa_oran_hesaplanmaz()
    {
        var cumartesi = new DateOnly(2026, 11, 21);

        var r = IstatistikHesaplayici.Hesapla([], TekDoktor, cumartesi, cumartesi, Ayar, new DateTime(2026, 11, 22));

        Assert.Equal(0, r.Kapasite);       // hafta sonu kapalı
        Assert.Null(r.Doluluk);
        Assert.Null(r.IptalOrani);
        Assert.Null(r.GelmemeOrani);
    }

    [Fact]
    public void Yogunluk_gun_ve_saat_kovasina_yalnizca_aktif_randevulari_sayar()
    {
        var gun = new DateOnly(2026, 11, 17); // salı
        var randevular = new List<RandevuOzeti>
        {
            R(1, new DateTime(2026, 11, 17, 10, 0, 0), RandevuDurumu.Onaylandi),
            R(2, new DateTime(2026, 11, 17, 10, 40, 0), RandevuDurumu.Beklemede),
            R(3, new DateTime(2026, 11, 17, 10, 20, 0), RandevuDurumu.IptalEdildi)
        };

        var r = IstatistikHesaplayici.Hesapla(randevular, TekDoktor, gun, gun, Ayar, gun.ToDateTime(TimeOnly.MinValue));

        Assert.Equal(2, r.Yogunluk.Single(h => h.Gun == DayOfWeek.Tuesday && h.Saat == 10).Sayi);
        Assert.Equal(5 * 8, r.Yogunluk.Count);          // 5 iş günü × 8 mesai saati (09–16)
        Assert.DoesNotContain(DayOfWeek.Saturday, r.Gunler);
        Assert.Equal(new GunlukSayi(gun, 2, 1), Assert.Single(r.Gunluk));
    }

    [Fact]
    public async Task Ornek_veriyle_Ekim_2026_istatistikleri()
    {
        await using var db = _vt.YeniContext();
        var servis = new IstatistikService(new EfIstatistikRepository(db), new SabitSaat(new DateTime(2026, 11, 17, 10, 0, 0)),
            Options.Create(new TakvimAyarlari()));

        var r = (await servis.RaporAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), null)).Veri!;

        Assert.Equal(10, r.Toplam);
        Assert.Equal(1, r.Iptal);
        Assert.Equal(3, r.Tamamlanan);
        Assert.Equal(1, r.Gelmedi);
        Assert.Equal(5, r.Sonuclanmamis);         // 20–23 Ekim randevularının sonucu girilmemiş
        Assert.Equal(10 * 22 * 21, r.Kapasite);   // 10 doktor × 22 iş günü × 21 slot
        Assert.Equal(0.25, r.GelmemeOrani);
        Assert.Equal(1, r.Yogunluk.Single(h => h.Gun == DayOfWeek.Tuesday && h.Saat == 9).Sayi); // 20.10 09:00

        var dahiliye = r.Bolumler.Single(b => b.BolumAdi == "Dahiliye");
        Assert.Equal((2, 1, 2 * 22 * 21), (dahiliye.Toplam, dahiliye.Aktif, dahiliye.Kapasite));
    }

    [Theory]
    [InlineData("2026-10-31", "2026-10-01", "önce")]
    [InlineData("2025-01-01", "2026-10-01", "366")]
    public void Gecersiz_tarih_araligi_reddedilir(string bas, string bit, string beklenen)
    {
        Assert.Contains(beklenen, IstatistikService.AraligiDenetle(DateOnly.Parse(bas), DateOnly.Parse(bit)));
    }

    // ---------------- Sıra panosu ----------------

    [Fact]
    public void Sira_panosu_su_anki_siradaki_kalan_ve_beklemeyi_hesaplar()
    {
        var simdi = new DateTime(2026, 11, 16, 10, 5, 0);
        var bugun = new List<RandevuOzeti>
        {
            R(1, new DateTime(2026, 11, 16, 9, 40, 0), RandevuDurumu.Tamamlandi),
            R(2, new DateTime(2026, 11, 16, 10, 0, 0), RandevuDurumu.Onaylandi, ad: "İpek", soyad: "Şahin"),
            R(3, new DateTime(2026, 11, 16, 10, 40, 0), RandevuDurumu.Beklemede, ad: "Can", soyad: "Kurt"),
            R(4, new DateTime(2026, 11, 16, 11, 0, 0), RandevuDurumu.IptalEdildi),
            R(5, new DateTime(2026, 11, 16, 11, 20, 0), RandevuDurumu.Onaylandi)
        };

        var s = Assert.Single(IstatistikHesaplayici.SiraPanosu(bugun, TekDoktor, simdi));

        Assert.Equal(2, s.SuAnki!.RandevuId);
        Assert.Equal("İ*** Ş***", s.SuAnki.MaskeliAd);
        Assert.Equal(3, s.Siradaki!.RandevuId);
        Assert.Equal(35, s.TahminiBeklemeDakika);
        Assert.Equal(2, s.KalanRandevu);
        Assert.Equal(4, s.BugunToplam);   // iptal hariç
        Assert.Equal(1, s.Tamamlanan);
    }

    [Fact]
    public void Randevusu_olmayan_doktor_bos_gorunur()
    {
        var s = Assert.Single(IstatistikHesaplayici.SiraPanosu([], TekDoktor, new DateTime(2026, 11, 16, 10, 0, 0)));

        Assert.Null(s.SuAnki);
        Assert.Null(s.Siradaki);
        Assert.Null(s.TahminiBeklemeDakika);
        Assert.Equal(0, s.KalanRandevu);
    }

    // ---------------- CSV ----------------

    [Fact]
    public void Csv_ayraci_tirnak_ve_satir_sonlari_kacislanir()
    {
        var csv = CsvYazici.Yaz([["Bölüm", "Not"], ["Kulak; Burun", "\"özel\" durum"]]);

        Assert.Equal("Bölüm;Not\r\n\"Kulak; Burun\";\"\"\"özel\"\" durum\"\r\n", csv);
    }

    [Theory]
    [InlineData("=1+1")]
    [InlineData("+90 555")]
    [InlineData("-2")]
    [InlineData("@SUM(A1)")]
    public void Formul_gibi_baslayan_hucre_duz_metne_cevrilir(string hucre)
    {
        Assert.StartsWith("'", CsvYazici.Hucre(hucre));
    }

    [Fact]
    public void Csv_UTF8_BOM_ile_baslar_ve_Turkce_karakterleri_korur()
    {
        var bayt = CsvYazici.Bayt("Gögüs;İç\r\n");

        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bayt[..3]);
        Assert.Equal("Gögüs;İç\r\n", Encoding.UTF8.GetString(bayt[3..]));
    }

    [Fact]
    public async Task Randevu_csv_hasta_adi_icermez()
    {
        await using var db = _vt.YeniContext();
        var servis = new IstatistikService(new EfIstatistikRepository(db), new SabitSaat(new DateTime(2026, 11, 17)),
            Options.Create(new TakvimAyarlari()));

        var csv = (await servis.RandevularCsvAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), null)).Veri!;

        Assert.Equal(11, csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Length); // başlık + 10 randevu
        Assert.DoesNotContain("Deniz", csv);   // hasta1'in adı
        Assert.Contains("İptal edildi", csv);
    }
}
