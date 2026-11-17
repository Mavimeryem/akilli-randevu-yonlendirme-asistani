namespace AkilliRandevu.Business.Bildirimler;

/// <summary>
/// E-posta servisi (Brevo REST API) ayarları. appsettings.json → "Eposta" bölümünden okunur.
/// API anahtarı ASLA dosyaya yazılmaz; ortam değişkeninden gelir: Eposta__ApiAnahtari
/// </summary>
public class EpostaAyarlari
{
    public string? ApiAnahtari { get; set; }
    public string ApiAdresi { get; set; } = "https://api.brevo.com/v3/smtp/email";
    public string? GondericiAdres { get; set; }
    public string GondericiAd { get; set; } = "Akıllı Randevu";

    /// <summary>
    /// Doluysa tüm e-postalar bu adrese gider (demo/test). Sentetik kullanıcıların @ornek.test
    /// adresleri gerçek olmadığı için demo ortamında gerçek bir adrese yönlendirilir.
    /// </summary>
    public string? YonlendirmeAdresi { get; set; }

    /// <summary>Tek bir HTTP denemesinin zaman aşımı.</summary>
    public int ZamanAsimiSaniye { get; set; } = 10;

    /// <summary>Bir gönderimde geçici hatalarda (429, 5xx, zaman aşımı) yapılacak en fazla HTTP denemesi.</summary>
    public int EnFazlaDeneme { get; set; } = 3;

    /// <summary>Retry-After başlığına uyulurken beklenecek en uzun süre.</summary>
    public int EnFazlaBeklemeSaniye { get; set; } = 30;

    public bool Yapilandirildi =>
        !string.IsNullOrWhiteSpace(ApiAnahtari) && !string.IsNullOrWhiteSpace(GondericiAdres);
}

/// <summary>Arka plan bildirim servisi ayarları. appsettings.json → "Bildirim" bölümü.</summary>
public class BildirimAyarlari
{
    public bool ArkaPlanEtkin { get; set; } = true;

    /// <summary>Arka plan servisinin kaç saniyede bir çalışacağı.</summary>
    public int TurAraligiSaniye { get; set; } = 60;

    /// <summary>Randevudan kaç saat önce hatırlatma gönderileceği.</summary>
    public int HatirlatmaSaatOnce { get; set; } = 24;

    /// <summary>Bir turda gönderilecek en fazla e-posta (servisin hız sınırını aşmamak için).</summary>
    public int TurBasinaEnFazlaEposta { get; set; } = 20;

    /// <summary>Bir e-postanın kuyrukta toplam kaç kez deneneceği; sonra "Başarısız" olur.</summary>
    public int EnFazlaKuyrukDenemesi { get; set; } = 5;
}
