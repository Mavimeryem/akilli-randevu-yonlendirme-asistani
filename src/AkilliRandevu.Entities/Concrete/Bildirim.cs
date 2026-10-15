using AkilliRandevu.Entities.Common;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Kullanıcıya gönderilen bildirim (randevu hatırlatma, iptal vb.). Kullanici 1-N Bildirim.
/// </summary>
public class Bildirim : BaseEntity
{
    public int KullaniciId { get; set; }
    public string Baslik { get; set; } = null!;
    public string Mesaj { get; set; } = null!;
    public bool OkunduMu { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
}
