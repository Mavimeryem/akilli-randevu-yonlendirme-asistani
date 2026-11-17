using AkilliRandevu.Business.Concrete;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.Business.Yonlendirme;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using AkilliRandevu.Tests.Yardimcilar;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Tests.Kurallar;

/// <summary>Hafta 13 – kritik iş kuralları için birim testleri (veritabanı yok, saf mantık).</summary>
public class KritikKurallarTestleri
{
    // ---------------- Slot üretici ----------------

    [Fact]
    public void Hafta_ici_21_slot_uretilir_ogle_arasi_atlanir()
    {
        var slotlar = SlotUretici.Araliklar(new DateOnly(2026, 11, 16), new TakvimAyarlari());

        Assert.Equal(21, slotlar.Count);
        Assert.Equal(new TimeOnly(9, 0), TimeOnly.FromDateTime(slotlar[0].Baslangic));
        Assert.Equal(new TimeOnly(16, 40), TimeOnly.FromDateTime(slotlar[^1].Baslangic));
        Assert.DoesNotContain(slotlar, s => s.Baslangic.Hour == 12);
    }

    [Fact]
    public void Hafta_sonu_kapaliysa_slot_yok_aciksa_var()
    {
        var cumartesi = new DateOnly(2026, 11, 21);
        Assert.Empty(SlotUretici.Araliklar(cumartesi, new TakvimAyarlari()));
        Assert.Equal(21, SlotUretici.Araliklar(cumartesi, new TakvimAyarlari { HaftaSonuAcik = true }).Count);
    }

    [Fact]
    public void Slot_suresi_degisince_slot_sayisi_degisir()
    {
        // 30 dk: 09:00–12:00 → 6, 13:00–17:00 → 8
        Assert.Equal(14, SlotUretici.Araliklar(new DateOnly(2026, 11, 16), new TakvimAyarlari { SlotDakika = 30 }).Count);
    }

    // ---------------- Randevu durum kuralları ----------------

    private static readonly TakvimAyarlari Ayar = new();
    private static readonly DateTime Simdi = new(2026, 11, 16, 10, 0, 0);

    private static Randevu Randevu(DateTime baslangic, RandevuDurumu durum = RandevuDurumu.Onaylandi) =>
        new() { Id = 1, HastaId = 1, DoktorId = 5, BaslangicZamani = baslangic, BitisZamani = baslangic.AddMinutes(20), Durum = durum };

    private static readonly Islemci Hasta = new(Rol.Hasta, HastaId: 1);
    private static readonly Islemci Doktor = new(Rol.Doktor, DoktorId: 5);

    [Fact]
    public void Hasta_tam_iki_saat_kala_iptal_edebilir_bir_dakika_eksikte_edemez()
    {
        Assert.Null(RandevuDurumKurallari.Denetle(Randevu(Simdi.AddHours(2)), RandevuDurumu.IptalEdildi, Hasta, Simdi, Ayar));
        Assert.NotNull(RandevuDurumKurallari.Denetle(Randevu(Simdi.AddMinutes(119)), RandevuDurumu.IptalEdildi, Hasta, Simdi, Ayar));
    }

    [Fact]
    public void Doktor_son_dakikada_da_iptal_edebilir_ama_baslamis_randevuyu_edemez()
    {
        Assert.Null(RandevuDurumKurallari.Denetle(Randevu(Simdi.AddMinutes(5)), RandevuDurumu.IptalEdildi, Doktor, Simdi, Ayar));
        Assert.NotNull(RandevuDurumKurallari.Denetle(Randevu(Simdi), RandevuDurumu.IptalEdildi, Doktor, Simdi, Ayar));
    }

    [Fact]
    public void Sonuc_yalnizca_doktor_tarafindan_ve_saat_geldikten_sonra_girilir()
    {
        Assert.NotNull(RandevuDurumKurallari.Denetle(Randevu(Simdi.AddMinutes(1)), RandevuDurumu.Tamamlandi, Doktor, Simdi, Ayar));
        Assert.Null(RandevuDurumKurallari.Denetle(Randevu(Simdi), RandevuDurumu.Tamamlandi, Doktor, Simdi, Ayar));
        Assert.NotNull(RandevuDurumKurallari.Denetle(Randevu(Simdi.AddHours(-1)), RandevuDurumu.Gelmedi, new Islemci(Rol.Yonetici), Simdi, Ayar));
    }

    [Theory]
    [InlineData(RandevuDurumu.Tamamlandi)]
    [InlineData(RandevuDurumu.Gelmedi)]
    [InlineData(RandevuDurumu.IptalEdildi)]
    public void Son_durumlardan_cikis_yoktur(RandevuDurumu son)
    {
        var r = Randevu(Simdi.AddDays(1), son);
        Assert.Empty(RandevuDurumKurallari.IzinVerilenler(r, new Islemci(Rol.Yonetici), Simdi, Ayar));
    }

    [Fact]
    public void Sahiplik_baska_hasta_ve_baska_doktor_dokunamaz()
    {
        var r = Randevu(Simdi.AddDays(1));
        Assert.NotNull(RandevuDurumKurallari.Denetle(r, RandevuDurumu.IptalEdildi, new Islemci(Rol.Hasta, HastaId: 2), Simdi, Ayar));
        Assert.NotNull(RandevuDurumKurallari.Denetle(r, RandevuDurumu.Onaylandi, new Islemci(Rol.Doktor, DoktorId: 6), Simdi, Ayar));
    }

    // ---------------- Yönlendirme motoru sınırları ----------------

    private static readonly Bolum Dahiliye = new() { Id = 1, Ad = "Dahiliye" };
    private static readonly Bolum Kardiyoloji = new() { Id = 2, Ad = "Kardiyoloji" };
    private static readonly Bolum Gogus = new() { Id = 8, Ad = "Göğüs Hastalıkları" };

    private static List<BelirtiBolum> Eslesmeler() =>
    [
        new() { BelirtiId = BelirtiKodlari.GogusAgrisi, BolumId = 2, AgirlikPuani = 9, Bolum = Kardiyoloji },
        new() { BelirtiId = BelirtiKodlari.GogusAgrisi, BolumId = 8, AgirlikPuani = 6, Bolum = Gogus },
        new() { BelirtiId = BelirtiKodlari.NefesDarligi, BolumId = 8, AgirlikPuani = 8, Bolum = Gogus },
        new() { BelirtiId = BelirtiKodlari.NefesDarligi, BolumId = 2, AgirlikPuani = 7, Bolum = Kardiyoloji },
    ];

    [Theory]
    [InlineData(6, AciliyetSeviyesi.Dusuk)]
    [InlineData(7, AciliyetSeviyesi.Acil)]
    public void Gogus_agrisi_7_ve_ustu_acildir(int siddet, AciliyetSeviyesi beklenen)
    {
        var s = YonlendirmeMotoru.Degerlendir([new(BelirtiKodlari.GogusAgrisi, siddet)], Eslesmeler(), Dahiliye);
        Assert.Equal(beklenen, s.Aciliyet);
    }

    [Theory]
    [InlineData(7, false)]
    [InlineData(8, true)]
    public void Tek_basina_nefes_darligi_8_ve_ustu_acildir(int siddet, bool acil)
    {
        var s = YonlendirmeMotoru.Degerlendir([new(BelirtiKodlari.NefesDarligi, siddet)], Eslesmeler(), Dahiliye);
        Assert.Equal(acil, s.AcilMi);
    }

    [Fact]
    public void Eslesme_yoksa_belirsiz_ve_Dahiliye_onerilir()
    {
        var s = YonlendirmeMotoru.Degerlendir([new(99, 5)], [], Dahiliye);

        Assert.True(s.Belirsiz);
        Assert.Equal(1, s.EnUygun!.BolumId);
        Assert.Equal(0, s.EnUygun.Puan);
    }

    [Fact]
    public void Iki_bolum_basa_bas_ise_sonuc_belirsizdir()
    {
        // Göğüs ağrısı 6: K 54, G 36 / Nefes darlığı 2: G 16, K 14 → K 68, G 52 → %57 / %43 → fark 14 → belirsiz değil
        // Göğüs ağrısı 2 + nefes darlığı 2: K 18+14=32, G 12+16=28 → %53 / %47 → fark 6 < 10 → belirsiz
        var s = YonlendirmeMotoru.Degerlendir(
            [new(BelirtiKodlari.GogusAgrisi, 2), new(BelirtiKodlari.NefesDarligi, 2)], Eslesmeler(), Dahiliye);

        Assert.True(s.Belirsiz);
        Assert.Equal("Dahiliye", s.EnUygun!.BolumAdi);
    }

    // ---------------- Parola hash ----------------

    [Fact]
    public void Ayni_sifre_her_seferinde_farkli_hash_uretir_ve_dogrulanir()
    {
        var h = new Pbkdf2SifreHasher();
        var a = h.Hashle("Demo1234");
        var b = h.Hashle("Demo1234");

        Assert.NotEqual(a, b);                    // tuz farklı
        Assert.StartsWith("PBKDF2-SHA256$600000$", a);
        Assert.True(h.Dogrula("Demo1234", a));
        Assert.True(h.Dogrula("Demo1234", b));
        Assert.False(h.Dogrula("demo1234", a));   // büyük/küçük harf duyarlı
    }

    [Theory]
    [InlineData("SENTETIK-HASH")]
    [InlineData("PBKDF2-SHA256$600000$bozuk!!$bozuk!!")]
    [InlineData("MD5$1$a$b")]
    [InlineData("")]
    public void Gecersiz_hash_bicimi_istisna_firlatmaz_false_doner(string hash)
    {
        Assert.False(new Pbkdf2SifreHasher().Dogrula("Demo1234", hash));
    }

    // ---------------- Giriş deneme kilidi ----------------

    [Fact]
    public void Dort_hata_kilitlemez_besinci_kilitler_15_dakika_sonra_acilir()
    {
        var zaman = new AyarlanabilirZaman(new DateTimeOffset(2026, 11, 16, 7, 0, 0, TimeSpan.Zero));
        var t = new BellekGirisDenemeTakipcisi(new MemoryCache(new MemoryCacheOptions()), zaman);

        for (int i = 0; i < 4; i++)
            t.BasarisizKaydet("a@b.c");
        Assert.Null(t.KalanKilitSuresi("a@b.c"));

        t.BasarisizKaydet("a@b.c");
        Assert.Equal(TimeSpan.FromMinutes(15), t.KalanKilitSuresi("a@b.c"));

        zaman.IleriAl(TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(1)));
        Assert.Null(t.KalanKilitSuresi("a@b.c"));

        t.BasarisizKaydet("a@b.c");            // kilit dolduktan sonra sayaç sıfırdan başlar
        Assert.Null(t.KalanKilitSuresi("a@b.c"));
    }

    [Fact]
    public void Basarili_giris_sayaci_sifirlar()
    {
        var t = new BellekGirisDenemeTakipcisi(new MemoryCache(new MemoryCacheOptions()), TimeProvider.System);
        for (int i = 0; i < 4; i++)
            t.BasarisizKaydet("x@y.z");
        t.Sifirla("x@y.z");
        t.BasarisizKaydet("x@y.z");
        Assert.Null(t.KalanKilitSuresi("x@y.z"));
    }

    // ---------------- Poliklinik saati (regresyon: H-03) ----------------

    [Fact]
    public void Sunucu_UTCde_calissa_da_poliklinik_saati_Istanbul_saatidir()
    {
        var zaman = new AyarlanabilirZaman(new DateTimeOffset(2026, 11, 17, 6, 30, 0, TimeSpan.Zero));
        var saat = new PoliklinikSaati(zaman, Options.Create(new TakvimAyarlari()));

        Assert.Equal(new DateTime(2026, 11, 17, 9, 30, 0), saat.Simdi);
        Assert.Equal(new DateTime(2026, 11, 18, 0, 30, 0), saat.YereleCevir(new DateTime(2026, 11, 17, 21, 30, 0)));
        Assert.Equal(new DateTime(2026, 11, 17, 6, 0, 0), saat.UtcyeCevir(new DateTime(2026, 11, 17, 9, 0, 0)));
    }
}
