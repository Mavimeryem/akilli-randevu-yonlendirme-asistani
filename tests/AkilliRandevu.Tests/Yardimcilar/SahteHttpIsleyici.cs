using System.Net;

namespace AkilliRandevu.Tests.Yardimcilar;

/// <summary>
/// Gerçek ağa çıkmadan HTTP yanıtlarını taklit eder. Her istek için sıradaki yanıtı döner
/// ve gelen istekleri (başlık + gövde) inceleme için saklar.
/// </summary>
public sealed class SahteHttpIsleyici : HttpMessageHandler
{
    private readonly Queue<Func<CancellationToken, Task<HttpResponseMessage>>> _yanitlar = new();

    public List<(HttpRequestMessage Istek, string Govde)> Istekler { get; } = new();

    public SahteHttpIsleyici Yanit(HttpStatusCode kod, string govde = "{}", Action<HttpResponseMessage>? ayarla = null)
    {
        _yanitlar.Enqueue(_ =>
        {
            var y = new HttpResponseMessage(kod) { Content = new StringContent(govde) };
            ayarla?.Invoke(y);
            return Task.FromResult(y);
        });
        return this;
    }

    /// <summary>İptal edilene kadar yanıt vermeyen sunucu (zaman aşımı senaryosu).</summary>
    public SahteHttpIsleyici AsiliKal()
    {
        _yanitlar.Enqueue(async ct =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            throw new InvalidOperationException("ulaşılamaz");
        });
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var govde = request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct);
        Istekler.Add((request, govde));
        if (_yanitlar.Count == 0)
            throw new InvalidOperationException("Beklenmeyen HTTP isteği.");
        return await _yanitlar.Dequeue()(ct);
    }
}
