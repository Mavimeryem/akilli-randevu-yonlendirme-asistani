namespace AkilliRandevu.Business.Bildirimler;

public record EpostaMesaji(string Alici, string Konu, string Metin);

public enum EpostaSonucTuru
{
    Gonderildi,
    /// <summary>Tekrar denenebilir: zaman aşımı, bağlantı hatası, 429 hız sınırı, 5xx.</summary>
    GeciciHata,
    /// <summary>Tekrar denemek işe yaramaz: geçersiz adres, yanlış anahtar (4xx).</summary>
    KaliciHata,
    /// <summary>API anahtarı / gönderici adresi ayarlanmamış.</summary>
    Yapilandirilmadi
}

public record EpostaSonucu(EpostaSonucTuru Tur, int DenemeSayisi, string? MesajId = null, string? Hata = null)
{
    public bool Basarili => Tur == EpostaSonucTuru.Gonderildi;
}

public interface IEpostaGonderici
{
    Task<EpostaSonucu> GonderAsync(EpostaMesaji mesaj, CancellationToken ct = default);
}
