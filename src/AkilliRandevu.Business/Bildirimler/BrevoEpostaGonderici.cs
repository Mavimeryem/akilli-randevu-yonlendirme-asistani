using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.Bildirimler;

/// <summary>
/// Brevo transactional e-posta REST API istemcisi.
///   POST https://api.brevo.com/v3/smtp/email
///   Başlık: api-key: &lt;anahtar&gt;     Gövde: { sender, to[], subject, textContent }
///   201 → { "messageId": "..." }     4xx/5xx → { "code": "...", "message": "..." }
///
/// Hata yönetimi:
///   - Her deneme ZamanAsimiSaniye ile sınırlı (asılı kalan istek yok).
///   - 429 (hız sınırı), 408, 5xx, zaman aşımı, bağlantı hatası → geçici; Retry-After başlığına uyulur,
///     yoksa üstel bekleme (1 sn, 2 sn, 4 sn …); en fazla EnFazlaDeneme kez denenir.
///   - Diğer 4xx (ör. 400 geçersiz adres, 401 yanlış anahtar) → kalıcı; tekrar denenmez.
///   - API anahtarı loglara ve hata mesajlarına asla yazılmaz.
/// </summary>
public sealed class BrevoEpostaGonderici : IEpostaGonderici
{
    private readonly HttpClient _http;
    private readonly EpostaAyarlari _ayar;
    private readonly ILogger<BrevoEpostaGonderici> _log;

    /// <summary>Denemeler arası bekleme. Testlerde gerçekten beklememek için değiştirilir.</summary>
    public Func<TimeSpan, CancellationToken, Task> Bekle { get; init; } = Task.Delay;

    public BrevoEpostaGonderici(HttpClient http, IOptions<EpostaAyarlari> ayar, ILogger<BrevoEpostaGonderici> log)
    {
        _http = http;
        _ayar = ayar.Value;
        _log = log;
    }

    public async Task<EpostaSonucu> GonderAsync(EpostaMesaji mesaj, CancellationToken ct = default)
    {
        if (!_ayar.Yapilandirildi)
            return new EpostaSonucu(EpostaSonucTuru.Yapilandirilmadi, 0, Hata: "E-posta servisi yapılandırılmamış.");

        var govde = new
        {
            sender = new { name = _ayar.GondericiAd, email = _ayar.GondericiAdres },
            to = new[] { new { email = mesaj.Alici } },
            subject = mesaj.Konu,
            textContent = mesaj.Metin
        };

        string? sonHata = null;
        int enFazla = Math.Max(1, _ayar.EnFazlaDeneme);

        for (int deneme = 1; deneme <= enFazla; deneme++)
        {
            TimeSpan? bekleme = null;
            try
            {
                using var istek = new HttpRequestMessage(HttpMethod.Post, _ayar.ApiAdresi)
                {
                    Content = JsonContent.Create(govde)
                };
                istek.Headers.Add("api-key", _ayar.ApiAnahtari);
                istek.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var zamanAsimi = CancellationTokenSource.CreateLinkedTokenSource(ct);
                zamanAsimi.CancelAfter(TimeSpan.FromSeconds(_ayar.ZamanAsimiSaniye));

                using var yanit = await _http.SendAsync(istek, zamanAsimi.Token);
                var icerik = await yanit.Content.ReadAsStringAsync(zamanAsimi.Token);

                if (yanit.IsSuccessStatusCode)
                    return new EpostaSonucu(EpostaSonucTuru.Gonderildi, deneme, MesajId: MesajIdOku(icerik));

                sonHata = $"HTTP {(int)yanit.StatusCode}: {HataMesajiOku(icerik)}";

                if (!GeciciMi(yanit.StatusCode))
                {
                    _log.LogWarning("E-posta kalıcı hata ile reddedildi: {Hata}", sonHata);
                    return new EpostaSonucu(EpostaSonucTuru.KaliciHata, deneme, Hata: sonHata);
                }

                bekleme = RetryAfterOku(yanit);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                sonHata = $"Zaman aşımı ({_ayar.ZamanAsimiSaniye} sn).";
            }
            catch (HttpRequestException ex)
            {
                sonHata = $"Bağlantı hatası: {ex.Message}";
            }

            _log.LogWarning("E-posta denemesi {Deneme}/{EnFazla} başarısız: {Hata}", deneme, enFazla, sonHata);

            if (deneme < enFazla)
            {
                var sure = bekleme ?? TimeSpan.FromSeconds(Math.Pow(2, deneme - 1));
                var tavan = TimeSpan.FromSeconds(_ayar.EnFazlaBeklemeSaniye);
                await Bekle(sure > tavan ? tavan : sure, ct);
            }
        }

        return new EpostaSonucu(EpostaSonucTuru.GeciciHata, enFazla, Hata: sonHata);
    }

    public static bool GeciciMi(HttpStatusCode kod) =>
        kod == HttpStatusCode.TooManyRequests || kod == HttpStatusCode.RequestTimeout || (int)kod >= 500;

    private static TimeSpan? RetryAfterOku(HttpResponseMessage yanit)
    {
        var ra = yanit.Headers.RetryAfter;
        if (ra?.Delta is { } delta)
            return delta;
        if (ra?.Date is { } tarih)
        {
            var kalan = tarih - DateTimeOffset.UtcNow;
            return kalan > TimeSpan.Zero ? kalan : TimeSpan.Zero;
        }
        return null;
    }

    // Başarılı yanıt: { "messageId": "<...@smtp-relay.mailin.fr>" }
    private static string? MesajIdOku(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("messageId", out var id) ? id.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Hata yanıtı: { "code": "invalid_parameter", "message": "email is not valid" }
    private static string HataMesajiOku(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var kok = doc.RootElement;
            var kod = kok.TryGetProperty("code", out var c) ? c.GetString() : null;
            var mesaj = kok.TryGetProperty("message", out var m) ? m.GetString() : null;
            return string.Join(" - ", new[] { kod, mesaj }.Where(x => !string.IsNullOrEmpty(x)));
        }
        catch (JsonException)
        {
            return json.Length > 200 ? json[..200] : json;
        }
    }
}
