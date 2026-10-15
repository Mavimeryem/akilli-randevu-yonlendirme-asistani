using AkilliRandevu.Entities.Common;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Sisteme giriş yapan her kişi (hasta, doktor, yönetici).
/// Kullanici 1-1 Hasta, Kullanici 1-1 Doktor, Kullanici 1-N Bildirim.
/// </summary>
public class Kullanici : BaseEntity
{
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public string Eposta { get; set; } = null!;
    public string SifreHash { get; set; } = null!;
    public string? Telefon { get; set; }
    public Rol Rol { get; set; }
    public bool AktifMi { get; set; } = true;

    public Hasta? Hasta { get; set; }
    public Doktor? Doktor { get; set; }
    public ICollection<Bildirim> Bildirimler { get; set; } = new List<Bildirim>();
}
