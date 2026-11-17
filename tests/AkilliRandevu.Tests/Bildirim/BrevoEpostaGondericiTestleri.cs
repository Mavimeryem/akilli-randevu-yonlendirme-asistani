using System.Net;
using System.Net.Http.Headers;
using AkilliRandevu.Business.Bildirimler;
using AkilliRandevu.Tests.Yardimcilar;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Tests.Bildirim;

/// <summary>Hafta 9: harici e-posta API'si çağrısı, yanıt ayrıştırma ve hata yönetimi.</summary>
public class BrevoEpostaGondericiTestleri
{
    private static readonly EpostaMesaji Mesaj = new("hasta@ornek.com", "Randevu hatırlatma", "Yarın 09:00 randevunuz var.");

    private static (BrevoEpostaGonderici Gonderici, SahteHttpIsleyici Http, List<TimeSpan> Beklemeler) Kur(
        Action<EpostaAyarlari>? ayarla = null)
    {
        var ayar = new EpostaAyarlari
        {
            ApiAnahtari = "test-anahtari",
            GondericiAdres = "noreply@ornek.com",
            ZamanAsimiSaniye = 1,
            EnFazlaDeneme = 3
        };
        ayarla?.Invoke(ayar);

        var http = new SahteHttpIsleyici();
        var beklemeler = new List<TimeSpan>();
        var gonderici = new BrevoEpostaGonderici(new HttpClient(http), Options.Create(ayar), NullLogger<BrevoEpostaGonderici>.Instance)
        {
            // Testte gerçekten beklemek yerine istenen süreyi kaydet.
            Bekle = (sure, _) => { beklemeler.Add(sure); return Task.CompletedTask; }
        };
        return (gonderici, http, beklemeler);
    }

    [Fact]
    public async Task Basarili_yanitta_mesaj_kimligi_ayristirilir_ve_anahtar_baslikta_gonderilir()
    {
        var (g, http, _) = Kur();
        http.Yanit(HttpStatusCode.Created, """{"messageId":"<abc123@smtp-relay.brevo.com>"}""");

        var sonuc = await g.GonderAsync(Mesaj);

        Assert.Equal(EpostaSonucTuru.Gonderildi, sonuc.Tur);
        Assert.Equal("<abc123@smtp-relay.brevo.com>", sonuc.MesajId);
        Assert.Equal(1, sonuc.DenemeSayisi);

        var (istek, govde) = Assert.Single(http.Istekler);
        Assert.Equal(HttpMethod.Post, istek.Method);
        Assert.Equal("test-anahtari", istek.Headers.GetValues("api-key").Single());
        Assert.Contains("\"hasta@ornek.com\"", govde);
        Assert.Contains("\"subject\":\"Randevu hatırlatma\"", govde.Replace("\\u0131", "ı"));
    }

    [Fact]
    public async Task Hiz_siniri_429da_Retry_After_suresi_kadar_beklenip_tekrar_denenir()
    {
        var (g, http, beklemeler) = Kur();
        http.Yanit(HttpStatusCode.TooManyRequests, """{"code":"too_many_requests","message":"rate limit"}""",
                y => y.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7)))
            .Yanit(HttpStatusCode.Created, """{"messageId":"m2"}""");

        var sonuc = await g.GonderAsync(Mesaj);

        Assert.True(sonuc.Basarili);
        Assert.Equal(2, sonuc.DenemeSayisi);
        Assert.Equal(new[] { TimeSpan.FromSeconds(7) }, beklemeler);
    }

    [Fact]
    public async Task Sunucu_hatasinda_ustel_beklemeyle_en_fazla_3_kez_denenir()
    {
        var (g, http, beklemeler) = Kur();
        http.Yanit(HttpStatusCode.InternalServerError)
            .Yanit(HttpStatusCode.BadGateway)
            .Yanit(HttpStatusCode.ServiceUnavailable);

        var sonuc = await g.GonderAsync(Mesaj);

        Assert.Equal(EpostaSonucTuru.GeciciHata, sonuc.Tur);
        Assert.Equal(3, sonuc.DenemeSayisi);
        Assert.Equal(3, http.Istekler.Count);
        Assert.Equal(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }, beklemeler);
        Assert.Contains("503", sonuc.Hata);
    }

    [Fact]
    public async Task Gecersiz_istek_400_kalici_hatadir_tekrar_denenmez_ve_hata_mesaji_ayristirilir()
    {
        var (g, http, beklemeler) = Kur();
        http.Yanit(HttpStatusCode.BadRequest, """{"code":"invalid_parameter","message":"email is not valid"}""");

        var sonuc = await g.GonderAsync(Mesaj);

        Assert.Equal(EpostaSonucTuru.KaliciHata, sonuc.Tur);
        Assert.Single(http.Istekler);
        Assert.Empty(beklemeler);
        Assert.Contains("invalid_parameter", sonuc.Hata);
        Assert.Contains("email is not valid", sonuc.Hata);
    }

    [Fact]
    public async Task Yanlis_anahtar_401_kalici_hatadir_ve_anahtar_hata_mesajinda_gorunmez()
    {
        var (g, http, _) = Kur();
        http.Yanit(HttpStatusCode.Unauthorized, """{"code":"unauthorized","message":"Key not found"}""");

        var sonuc = await g.GonderAsync(Mesaj);

        Assert.Equal(EpostaSonucTuru.KaliciHata, sonuc.Tur);
        Assert.DoesNotContain("test-anahtari", sonuc.Hata);
    }

    [Fact]
    public async Task Yanit_gelmezse_zaman_asimi_gecici_hata_olarak_doner()
    {
        var (g, http, _) = Kur(a => a.EnFazlaDeneme = 1);
        http.AsiliKal();

        var sonuc = await g.GonderAsync(Mesaj);

        Assert.Equal(EpostaSonucTuru.GeciciHata, sonuc.Tur);
        Assert.Contains("Zaman aşımı", sonuc.Hata);
    }

    [Fact]
    public async Task Anahtar_yoksa_hic_istek_atilmaz()
    {
        var (g, http, _) = Kur(a => a.ApiAnahtari = null);

        var sonuc = await g.GonderAsync(Mesaj);

        Assert.Equal(EpostaSonucTuru.Yapilandirilmadi, sonuc.Tur);
        Assert.Empty(http.Istekler);
    }
}
