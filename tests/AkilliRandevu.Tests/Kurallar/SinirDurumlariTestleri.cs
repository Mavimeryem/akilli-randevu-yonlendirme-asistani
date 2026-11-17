using AkilliRandevu.Business.Bildirimler;
using AkilliRandevu.Business.Concrete;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.DataAccess.Concrete;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using AkilliRandevu.Tests.Yardimcilar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Tests.Kurallar;

/// <summary>Hafta 13 – sınır ve hata senaryoları (gerçek veritabanıyla): randevu oluşturma ve kullanıcı kaydı/girişi.</summary>
public class SinirDurumlariTestleri : IDisposable
{
    private readonly TestVeritabani _vt = new();
    private readonly SabitSaat _saat = new(new DateTime(2026, 11, 16, 8, 0, 0)); // pazartesi

    public void Dispose() => _vt.Dispose();

    private RandevuService RandevuServisi(AppDbContext db) => new(
        new EfRandevuRepository(db), new EfDoktorRepository(db), new EfBolumRepository(db), new EfRepository<Hasta>(db),
        new EfBelirtiKaydiRepository(db),
        new BildirimService(new EfBildirimRepository(db), new EfKullaniciRepository(db), Options.Create(new EpostaAyarlari())),
        new EfIslemYoneticisi(db), Options.Create(new TakvimAyarlari()), _saat);

    // ---------------- Randevu oluşturma sınırları ----------------

    [Fact]
    public async Task Son_slot_alinabilir_mesai_bitisi_alinamaz()
    {
        await using var db = _vt.YeniContext();
        var s = RandevuServisi(db);

        Assert.True((await s.OlusturAsync(3, 2, new DateTime(2026, 11, 16, 16, 40, 0))).Basarili);
        Assert.False((await s.OlusturAsync(4, 2, new DateTime(2026, 11, 16, 17, 0, 0))).Basarili);
    }

    [Fact]
    public async Task Otuzuncu_gun_alinabilir_otuz_birinci_gun_alinamaz()
    {
        await using var db = _vt.YeniContext();
        var s = RandevuServisi(db);

        Assert.True((await s.OlusturAsync(3, 2, new DateTime(2026, 12, 16, 9, 0, 0))).Basarili);
        var r = await s.OlusturAsync(4, 2, new DateTime(2026, 12, 17, 9, 0, 0));
        Assert.Contains("30 gün", r.Hata);
    }

    [Fact]
    public async Task Hasta_ayni_saatte_baska_bolumden_de_randevu_alamaz()
    {
        await using var db = _vt.YeniContext();
        var s = RandevuServisi(db);
        var saat = new DateTime(2026, 11, 17, 10, 0, 0);

        Assert.True((await s.OlusturAsync(3, 2, saat)).Basarili);        // Dahiliye
        var r = await s.OlusturAsync(3, 4, saat);                         // Kardiyoloji, aynı saat
        Assert.Contains("Aynı saatte", r.Hata);
    }

    [Fact]
    public async Task Pasif_doktordan_randevu_alinamaz()
    {
        await using var db = _vt.YeniContext();
        var doktorKullanici = await db.Kullanicilar.SingleAsync(k => k.Id == 3); // doktor2
        doktorKullanici.AktifMi = false;
        await db.SaveChangesAsync();

        var r = await RandevuServisi(db).OlusturAsync(3, 2, new DateTime(2026, 11, 17, 10, 0, 0));

        Assert.False(r.Basarili);
    }

    [Fact]
    public async Task Olmayan_doktor_ve_olmayan_hasta_reddedilir()
    {
        await using var db = _vt.YeniContext();
        var s = RandevuServisi(db);

        Assert.False((await s.OlusturAsync(3, 999, new DateTime(2026, 11, 17, 10, 0, 0))).Basarili);
        Assert.False((await s.OlusturAsync(999, 2, new DateTime(2026, 11, 17, 10, 0, 0))).Basarili);
    }

    // ---------------- Kullanıcı kaydı ve girişi ----------------

    private static KullaniciService KullaniciServisi(AppDbContext db) => new(
        new EfKullaniciRepository(db), new EfRepository<Hasta>(db), new Pbkdf2SifreHasher(),
        new BellekGirisDenemeTakipcisi(new MemoryCache(new MemoryCacheOptions()), TimeProvider.System));

    private static KayitIstegi Istek(string eposta = "yeni@ornek.test", string sifre = "abcdefg1", string tc = "99000000050") =>
        new("Yeni", "Hasta", eposta, sifre, null, tc, new DateOnly(1999, 1, 1), Cinsiyet.Belirtilmedi);

    [Theory]
    [InlineData("abcdef1", false)]     // 7 karakter
    [InlineData("abcdefgh", false)]    // rakam yok
    [InlineData("12345678", false)]    // harf yok
    [InlineData("abcdefg1", true)]     // tam 8, harf + rakam
    public async Task Sifre_kurali_sinirlari(string sifre, bool gecerli)
    {
        await using var db = _vt.YeniContext();
        Assert.Equal(gecerli, (await KullaniciServisi(db).HastaKaydetAsync(Istek(sifre: sifre))).Basarili);
    }

    [Theory]
    [InlineData("9900000005")]     // 10 hane
    [InlineData("990000000500")]   // 12 hane
    [InlineData("99000000O50")]    // harf O
    public async Task Gecersiz_TC_reddedilir(string tc)
    {
        await using var db = _vt.YeniContext();
        Assert.False((await KullaniciServisi(db).HastaKaydetAsync(Istek(tc: tc))).Basarili);
    }

    [Fact]
    public async Task Ayni_eposta_buyuk_harfle_de_ikinci_kez_kaydedilemez()
    {
        await using var db = _vt.YeniContext();
        var s = KullaniciServisi(db);

        Assert.True((await s.HastaKaydetAsync(Istek(eposta: "Ayse@Ornek.Test"))).Basarili);
        var ikinci = await s.HastaKaydetAsync(Istek(eposta: "  AYSE@ornek.test ", tc: "99000000051"));
        Assert.Contains("e-posta", ikinci.Hatalar[0]);
    }

    [Fact]
    public async Task Giris_dogru_yanlis_ve_pasif_hesap()
    {
        await using var db = _vt.YeniContext();
        var s = KullaniciServisi(db);
        await s.HastaKaydetAsync(Istek());

        Assert.Equal(GirisDurumu.HataliBilgi, (await s.GirisYapAsync("yeni@ornek.test", "yanlis999")).Durum);
        Assert.Equal(GirisDurumu.HataliBilgi, (await s.GirisYapAsync("olmayan@ornek.test", "abcdefg1")).Durum);
        Assert.Equal(GirisDurumu.Basarili, (await s.GirisYapAsync("YENI@ornek.test", "abcdefg1")).Durum);

        var k = await db.Kullanicilar.SingleAsync(x => x.Eposta == "yeni@ornek.test");
        k.AktifMi = false;
        await db.SaveChangesAsync();
        Assert.Equal(GirisDurumu.HesapPasif, (await s.GirisYapAsync("yeni@ornek.test", "abcdefg1")).Durum);
    }

    [Fact]
    public async Task Ornek_kullanicilarin_yer_tutucu_hashi_ile_giris_yapilamaz()
    {
        // Demo şifreleri yalnızca geliştirme ortamında ayarlanır; migration'daki "SENTETIK-HASH" ile giriş olmamalı.
        await using var db = _vt.YeniContext();
        Assert.Equal(GirisDurumu.HataliBilgi, (await KullaniciServisi(db).GirisYapAsync("hasta1@ornek.test", "Demo1234")).Durum);
    }
}
