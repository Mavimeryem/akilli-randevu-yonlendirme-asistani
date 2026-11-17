using AkilliRandevu.Business.Bildirimler;
using AkilliRandevu.Business.Concrete;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Gecmis;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.DataAccess.Concrete;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using AkilliRandevu.Tests.Yardimcilar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Tests.Gecmis;

/// <summary>
/// Hafta 11: hasta geçmişi – erişim yetkisi, erişim izi, filtreleme, özet, doktor notu.
/// Örnek veri: hasta1 (kullanıcı 12) → randevular: 05.10 Nöroloji/doktor5 (tamamlandı, notlu), 20.10 Dahiliye/doktor1 (beklemede);
/// belirti kayıtları: 03.10 (öneri Nöroloji), 14.10 (öneri Dahiliye). doktor2'nin hiç hastası yok.
/// </summary>
public class HastaGecmisiTestleri : IDisposable
{
    private readonly TestVeritabani _vt = new();
    private readonly SabitSaat _saat = new(new DateTime(2026, 11, 17, 10, 0, 0));

    public void Dispose() => _vt.Dispose();

    private HastaGecmisiService Servis(AppDbContext db) => new(new EfHastaGecmisiRepository(db), _saat);

    private static readonly Islemci Hasta1 = new(Rol.Hasta, HastaId: 1);
    private static readonly Islemci Hasta2 = new(Rol.Hasta, HastaId: 2);
    private static readonly Islemci Doktor5 = new(Rol.Doktor, DoktorId: 5);   // kullanıcı 6
    private static readonly Islemci Doktor2 = new(Rol.Doktor, DoktorId: 2);   // kullanıcı 3, hastası yok
    private static readonly Islemci Yonetici = new(Rol.Yonetici);             // kullanıcı 1

    // ---------------- Erişim yetkisi ----------------

    [Fact]
    public async Task Hasta_kendi_gecmisini_gorur_baskasininkini_goremez()
    {
        await using var db = _vt.YeniContext();
        Assert.True((await Servis(db).GetirAsync(1, new(), Hasta1, 12)).Basarili);
        Assert.False((await Servis(db).GetirAsync(1, new(), Hasta2, 13)).Basarili);
    }

    [Fact]
    public async Task Doktor_yalnizca_randevu_iliskisi_olan_hastayi_gorur()
    {
        await using var db = _vt.YeniContext();
        Assert.True((await Servis(db).GetirAsync(1, new(), Doktor5, 6)).Basarili);
        Assert.False((await Servis(db).GetirAsync(1, new(), Doktor2, 3)).Basarili);
    }

    [Fact]
    public async Task Olmayan_hasta_ile_yetkisiz_hasta_ayni_cevabi_alir()
    {
        await using var db = _vt.YeniContext();
        var olmayan = await Servis(db).GetirAsync(999, new(), Doktor2, 3);
        var yetkisiz = await Servis(db).GetirAsync(1, new(), Doktor2, 3);
        Assert.Equal(olmayan.Hata, yetkisiz.Hata);
    }

    [Fact]
    public async Task Yonetici_tum_hastalari_gorur_ama_tibbi_icerik_gizlidir()
    {
        await using var db = _vt.YeniContext();
        var g = (await Servis(db).GetirAsync(1, new(), Yonetici, 1)).Veri!;

        Assert.True(g.TibbiIcerikGizli);
        Assert.Null(g.Yas);
        Assert.All(g.Olaylar, o =>
        {
            Assert.Null(o.Aciklama);
            Assert.Null(o.DoktorNotu);
            Assert.Null(o.YzOzeti);
            Assert.Empty(o.Belirtiler);
        });
    }

    [Fact]
    public async Task Doktor_tibbi_icerigi_ve_notlari_gorur()
    {
        await using var db = _vt.YeniContext();
        var g = (await Servis(db).GetirAsync(1, new(), Doktor5, 6)).Veri!;

        Assert.False(g.TibbiIcerikGizli);
        Assert.Contains(g.Olaylar, o => o.DoktorNotu is not null);
        Assert.Contains(g.Olaylar, o => o.Belirtiler.Count > 0);
        Assert.Contains(g.Olaylar, o => o.KendiRandevusu); // 05.10 randevusu doktor5'in
    }

    // ---------------- Erişim izi ----------------

    [Fact]
    public async Task Doktor_ve_yonetici_erisimi_kaydedilir_hastanin_kendi_erisimi_kaydedilmez()
    {
        await using var db = _vt.YeniContext();
        var s = Servis(db);

        await s.GetirAsync(1, new(), Hasta1, 12);
        Assert.Equal(0, await db.ErisimKayitlari.CountAsync());

        await s.GetirAsync(1, new(), Doktor5, 6);
        await s.GetirAsync(1, new(), Yonetici, 1);
        var izler = await db.ErisimKayitlari.OrderBy(e => e.Id).ToListAsync();
        Assert.Equal(2, izler.Count);
        Assert.Equal((6, Rol.Doktor), (izler[0].ErisenKullaniciId, izler[0].ErisenRol));
        Assert.Equal((1, Rol.Yonetici), (izler[1].ErisenKullaniciId, izler[1].ErisenRol));
    }

    [Fact]
    public async Task Yetkisiz_deneme_erisim_izi_birakmaz_ve_hasta_kendi_izlerini_gorur()
    {
        await using var db = _vt.YeniContext();
        var s = Servis(db);
        await s.GetirAsync(1, new(), Doktor2, 3);   // yetkisiz
        await s.GetirAsync(1, new(), Doktor5, 6);   // yetkili

        var kendi = await s.ErisimKayitlariAsync(1, Hasta1);
        Assert.Equal(6, Assert.Single(kendi.Veri!).ErisenKullaniciId);

        Assert.False((await s.ErisimKayitlariAsync(1, Hasta2)).Basarili);
        Assert.False((await s.ErisimKayitlariAsync(1, Doktor5)).Basarili);
    }

    // ---------------- Filtreler ----------------

    [Theory]
    [InlineData("2026-10-20", "2026-10-10", "önce")]
    [InlineData("2024-01-01", "2026-10-10", "2 yıl")]
    public void Gecersiz_tarih_araligi_reddedilir(string bas, string bit, string beklenen)
    {
        var hata = HastaGecmisiService.FiltreyiDenetle(new GecmisFiltresi(DateOnly.Parse(bas), DateOnly.Parse(bit)));
        Assert.Contains(beklenen, hata);
    }

    [Fact]
    public void Belirti_turunde_durum_filtresi_reddedilir()
    {
        Assert.NotNull(HastaGecmisiService.FiltreyiDenetle(new GecmisFiltresi(Tur: GecmisTuru.Belirti, Durum: RandevuDurumu.Tamamlandi)));
    }

    [Fact]
    public async Task Filtresiz_tum_olaylar_tarihe_gore_yeniden_eskiye_gelir()
    {
        await using var db = _vt.YeniContext();
        var g = (await Servis(db).GetirAsync(1, new(), Hasta1, 12)).Veri!;

        Assert.Equal(4, g.ToplamOlay);
        Assert.Equal(g.Olaylar.OrderByDescending(o => o.Tarih).Select(o => o.Tarih), g.Olaylar.Select(o => o.Tarih));
    }

    [Fact]
    public async Task Tur_durum_bolum_ve_tarih_filtreleri_uygulanir()
    {
        await using var db = _vt.YeniContext();
        var s = Servis(db);

        var randevular = (await s.GetirAsync(1, new(Tur: GecmisTuru.Randevu), Hasta1, 12)).Veri!;
        Assert.Equal(2, randevular.ToplamOlay);
        Assert.All(randevular.Olaylar, o => Assert.Equal(GecmisTuru.Randevu, o.Tur));

        var tamamlanan = (await s.GetirAsync(1, new(Durum: RandevuDurumu.Tamamlandi), Hasta1, 12)).Veri!;
        Assert.Equal(RandevuDurumu.Tamamlandi, Assert.Single(tamamlanan.Olaylar).RandevuDurumu);

        var noroloji = (await s.GetirAsync(1, new(BolumId: 3), Hasta1, 12)).Veri!;
        Assert.Equal(2, noroloji.ToplamOlay); // 05.10 randevu + 03.10 belirti kaydı

        var ekimSonu = (await s.GetirAsync(1, new(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 31)), Hasta1, 12)).Veri!;
        Assert.Equal(2, ekimSonu.ToplamOlay); // 14.10 belirti + 20.10 randevu
        Assert.All(ekimSonu.Olaylar, o => Assert.True(o.Tarih >= new DateTime(2026, 10, 10)));
    }

    [Fact]
    public async Task Ozet_filtreden_bagimsiz_tum_gecmisten_hesaplanir()
    {
        await using var db = _vt.YeniContext();
        var g = (await Servis(db).GetirAsync(1, new(Tur: GecmisTuru.Belirti), Hasta1, 12)).Veri!;

        Assert.Equal(2, g.Ozet.ToplamRandevu);
        Assert.Equal(1, g.Ozet.Tamamlanan);
        Assert.Equal(2, g.Ozet.BelirtiKaydi);
        Assert.Equal(new DateTime(2026, 10, 5, 9, 0, 0), g.Ozet.SonZiyaret);
    }

    [Fact]
    public async Task Doktor_hastalari_yalnizca_randevu_iliskisi_olanlardir()
    {
        await using var db = _vt.YeniContext();
        var s = Servis(db);

        Assert.Equal([1, 6], (await s.DoktorHastalariAsync(1)).Select(h => h.HastaId).Order());
        Assert.Empty(await s.DoktorHastalariAsync(2));
    }

    // ---------------- Doktor notu ----------------

    private RandevuService RandevuServisi(AppDbContext db, SabitSaat saat) => new(
        new EfRandevuRepository(db), new EfDoktorRepository(db), new EfBolumRepository(db), new EfRepository<Hasta>(db),
        new EfBelirtiKaydiRepository(db),
        new BildirimService(new EfBildirimRepository(db), new EfKullaniciRepository(db), Options.Create(new EpostaAyarlari())),
        new EfIslemYoneticisi(db), Options.Create(new TakvimAyarlari()), saat);

    [Fact]
    public async Task Doktor_kendi_gecmis_randevusuna_not_yazabilir()
    {
        await using var db = _vt.YeniContext();
        var sonuc = await RandevuServisi(db, _saat).DoktorNotuKaydetAsync(1, "  Kontrol muayenesi yapıldı.  ", Doktor5);

        Assert.True(sonuc.Basarili);
        Assert.Equal("Kontrol muayenesi yapıldı.", (await db.Randevular.AsNoTracking().SingleAsync(r => r.Id == 1)).DoktorNotu);
    }

    [Fact]
    public async Task Baska_doktorun_randevusuna_not_yazilamaz()
    {
        await using var db = _vt.YeniContext();
        Assert.False((await RandevuServisi(db, _saat).DoktorNotuKaydetAsync(1, "not", Doktor2)).Basarili);
    }

    [Fact]
    public async Task Randevu_saati_gelmeden_not_yazilamaz()
    {
        await using var db = _vt.YeniContext();
        var oncesi = new SabitSaat(new DateTime(2026, 10, 19, 10, 0, 0));
        // 7 numaralı randevu: 20.10.2026 09:00, doktor1
        var sonuc = await RandevuServisi(db, oncesi).DoktorNotuKaydetAsync(7, "not", new Islemci(Rol.Doktor, DoktorId: 1));

        Assert.False(sonuc.Basarili);
        Assert.Contains("saati geldikten sonra", sonuc.Hata);
    }

    [Fact]
    public async Task Iptal_edilmis_randevuya_not_yazilamaz()
    {
        await using var db = _vt.YeniContext();
        // 5 numaralı randevu: 13.10.2026, doktor1, iptal edilmiş
        Assert.False((await RandevuServisi(db, _saat).DoktorNotuKaydetAsync(5, "not", new Islemci(Rol.Doktor, DoktorId: 1))).Basarili);
    }
}
