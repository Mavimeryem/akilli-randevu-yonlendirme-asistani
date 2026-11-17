namespace AkilliRandevu.Business.YapayZeka;

/// <summary>
/// Yapay zekâ bileşeni ayarları. appsettings.json → "YapayZeka" bölümü.
/// API anahtarı DOSYADA DEĞİL: ortam değişkeni ANTHROPIC_API_KEY (ya da YapayZeka__ApiAnahtari).
/// </summary>
public class YapayZekaAyarlari
{
    public bool Etkin { get; set; } = true;
    public string? ApiAnahtari { get; set; }
    public string Model { get; set; } = "claude-opus-5-5";

    /// <summary>
    /// Düşünme derinliği: low | medium | high. Bu iş bir sınıflandırma (bölüm seçimi) olduğu için "low"
    /// yeterli; yanıt süresi ve maliyet düşer.
    /// </summary>
    public string Efor { get; set; } = "low";

    public int MaxTokens { get; set; } = 4000;
    public int ZamanAsimiSaniye { get; set; } = 45;

    /// <summary>
    /// Model isteği güvenlik gerekçesiyle reddederse aynı çağrıda yedek modelin yanıt vermesi
    /// (sunucu taraflı fallback). Kapatmak için false.
    /// </summary>
    public bool YedekModelEtkin { get; set; } = true;

    public string? EtkinAnahtar =>
        !string.IsNullOrWhiteSpace(ApiAnahtari) ? ApiAnahtari : Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

    public bool Yapilandirildi => Etkin && !string.IsNullOrWhiteSpace(EtkinAnahtar);
}
