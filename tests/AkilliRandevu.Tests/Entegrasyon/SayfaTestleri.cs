using System.Net;

namespace AkilliRandevu.Tests.Entegrasyon;

/// <summary>
/// Hafta 13 – entegrasyon: her rolün her sayfası gerçekten açılıyor mu (Razor görünümleri çalışma anında
/// derlenip veriyle çiziliyor), yetkisiz erişim engelleniyor mu?
/// </summary>
public class SayfaTestleri : IClassFixture<WebFabrikasi>
{
    private readonly WebFabrikasi _f;

    public SayfaTestleri(WebFabrikasi f)
    {
        _f = f;
    }

    public static TheoryData<string, string> RolSayfalari => new()
    {
        { "", "/" },
        { "", "/Hesap/Giris" },
        { "", "/Hesap/Kayit" },
        { "hasta1@ornek.test", "/Belirti/Giris" },
        { "hasta1@ornek.test", "/Belirti/Sonuc/1" },
        { "hasta1@ornek.test", "/Belirti/Kayitlarim" },
        { "hasta1@ornek.test", "/Randevu/Al" },
        { "hasta1@ornek.test", "/Randevu/Al?bolumId=1&doktorId=1" },
        { "hasta1@ornek.test", "/Hasta/Randevularim" },
        { "hasta1@ornek.test", "/Gecmis" },
        { "hasta1@ornek.test", "/Gecmis?tur=Randevu&bolumId=3" },
        { "hasta1@ornek.test", "/Bildirim" },
        { "doktor5@ornek.test", "/Doktor/Takvim" },
        { "doktor5@ornek.test", "/Doktor/Takvim?gun=2026-10-05" },
        { "doktor5@ornek.test", "/Gecmis/Hastalarim" },
        { "doktor5@ornek.test", "/Gecmis?hastaId=1" },
        { "doktor5@ornek.test", "/SiraPanosu" },
        { "yonetici@ornek.test", "/Yonetim/Kullanicilar" },
        { "yonetici@ornek.test", "/Yonetim/Randevular?gun=2026-10-20" },
        { "yonetici@ornek.test", "/Yonetim/EpostaGonderimleri" },
        { "yonetici@ornek.test", "/Gecmis?hastaId=1" },
        { "yonetici@ornek.test", "/Gecmis/ErisimKayitlari" },
        { "yonetici@ornek.test", "/Istatistik" },
        { "yonetici@ornek.test", "/Istatistik?baslangic=2026-10-01&bitis=2026-10-31" },
        { "yonetici@ornek.test", "/Istatistik/BolumlerCsv?baslangic=2026-10-01&bitis=2026-10-31" },
        { "yonetici@ornek.test", "/Istatistik/RandevularCsv?baslangic=2026-10-01&bitis=2026-10-31" },
        { "yonetici@ornek.test", "/SiraPanosu?bolumId=1" },
    };

    [Theory]
    [MemberData(nameof(RolSayfalari))]
    public async Task Sayfa_yetkili_rol_icin_acilir(string eposta, string adres)
    {
        var c = _f.Istemci();
        if (eposta != "")
            await c.GirisYapAsync(eposta);

        var y = await c.GetAsync(adres);

        Assert.Equal(HttpStatusCode.OK, y.StatusCode);
    }

    [Theory]
    [InlineData("/Hasta/Randevularim")]
    [InlineData("/Gecmis")]
    [InlineData("/Istatistik")]
    [InlineData("/Bildirim")]
    public async Task Giris_yapmamis_kullanici_giris_sayfasina_yonlenir(string adres)
    {
        var y = await _f.Istemci().GetAsync(adres);

        Assert.Equal(HttpStatusCode.Redirect, y.StatusCode);
        Assert.StartsWith("/Hesap/Giris", y.Headers.Location!.PathAndQuery);
    }

    [Theory]
    [InlineData("hasta1@ornek.test", "/Istatistik")]
    [InlineData("hasta1@ornek.test", "/SiraPanosu")]
    [InlineData("hasta1@ornek.test", "/Doktor/Takvim")]
    [InlineData("doktor5@ornek.test", "/Yonetim/Kullanicilar")]
    [InlineData("doktor5@ornek.test", "/Belirti/Giris")]
    public async Task Rolu_yetmeyen_kullanici_erisim_engellendi_sayfasina_yonlenir(string eposta, string adres)
    {
        var c = _f.Istemci();
        await c.GirisYapAsync(eposta);

        var y = await c.GetAsync(adres);

        Assert.Equal(HttpStatusCode.Redirect, y.StatusCode);
        Assert.StartsWith("/Hesap/ErisimEngellendi", y.Headers.Location!.PathAndQuery);
    }

    [Fact]
    public async Task Iliskisi_olmayan_doktor_hasta_gecmisini_goremez()
    {
        var c = _f.Istemci();
        await c.GirisYapAsync("doktor2@ornek.test");

        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Gecmis?hastaId=1")).StatusCode);
    }

    [Fact]
    public async Task Hasta_baskasinin_belirti_sonucunu_goremez()
    {
        var c = _f.Istemci();
        await c.GirisYapAsync("hasta2@ornek.test");

        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Belirti/Sonuc/1")).StatusCode);
    }

    [Fact]
    public async Task Antiforgery_tokeni_olmayan_POST_reddedilir()
    {
        var c = _f.Istemci();
        await c.GirisYapAsync("hasta1@ornek.test");

        var y = await c.PostAsync("/Randevu/DurumDegistir",
            new FormUrlEncodedContent([new("id", "7"), new("durum", "IptalEdildi")]));

        Assert.Equal(HttpStatusCode.BadRequest, y.StatusCode);
    }

    [Fact]
    public async Task Csv_dosya_olarak_iner()
    {
        var c = _f.Istemci();
        await c.GirisYapAsync("yonetici@ornek.test");

        var y = await c.GetAsync("/Istatistik/RandevularCsv?baslangic=2026-10-01&bitis=2026-10-31");

        Assert.Equal("text/csv", y.Content.Headers.ContentType!.MediaType);
        Assert.Contains("attachment", y.Content.Headers.ContentDisposition!.ToString());
    }
}
