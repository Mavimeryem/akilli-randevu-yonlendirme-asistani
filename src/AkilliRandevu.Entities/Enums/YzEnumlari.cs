namespace AkilliRandevu.Entities.Enums;

/// <summary>Yapay zekâ değerlendirmesinin sonucu.</summary>
public enum YzDurumu
{
    Basarili = 1,
    /// <summary>API hatası, zaman aşımı, bağlantı hatası.</summary>
    Hatali = 2,
    /// <summary>Yanıt geldi ama doğrulamadan geçmedi (boş, bozuk JSON, listede olmayan bölüm…).</summary>
    Gecersiz = 3,
    /// <summary>Model isteği güvenlik gerekçesiyle reddetti (stop_reason = refusal).</summary>
    Reddedildi = 4
}

/// <summary>Yapay zekânın kendi önerisine duyduğu güven (modelin beyanı).</summary>
public enum YzGuven
{
    Dusuk = 1,
    Orta = 2,
    Yuksek = 3
}
