using AkilliRandevu.Entities.Common;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Bir hastanın geçmişine hasta dışında birinin (doktor/yönetici) eriştiği her an kaydedilir (KVKK – erişim izi).
/// Hasta 1-N ErisimKaydi; Kullanici (erişen) 1-N ErisimKaydi. Hasta kendi sayfasında bu kayıtları görür.
/// </summary>
public class ErisimKaydi : BaseEntity
{
    public int HastaId { get; set; }
    public int ErisenKullaniciId { get; set; }
    public Rol ErisenRol { get; set; }
    public string Islem { get; set; } = null!;

    public Hasta Hasta { get; set; } = null!;
    public Kullanici Erisen { get; set; } = null!;
}
