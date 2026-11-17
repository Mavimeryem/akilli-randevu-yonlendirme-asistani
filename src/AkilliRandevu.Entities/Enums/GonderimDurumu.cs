namespace AkilliRandevu.Entities.Enums;

public enum GonderimDurumu
{
    /// <summary>Kuyrukta, arka plan servisi gönderecek.</summary>
    Bekliyor = 1,
    Gonderildi = 2,
    /// <summary>Kalıcı hata (ör. geçersiz adres) ya da deneme hakkı bitti.</summary>
    Basarisiz = 3,
    /// <summary>E-posta servisi yapılandırılmadığı için gönderilmedi; bildirim uygulama içinde görünür.</summary>
    Atlandi = 4
}
