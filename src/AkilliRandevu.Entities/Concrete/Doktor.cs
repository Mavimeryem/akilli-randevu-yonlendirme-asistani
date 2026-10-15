using AkilliRandevu.Entities.Common;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Bir bölümde çalışan doktor. Bolum 1-N Doktor, Doktor 1-N Randevu.
/// </summary>
public class Doktor : BaseEntity
{
    public int KullaniciId { get; set; }
    public int BolumId { get; set; }
    public string Unvan { get; set; } = null!;

    public Kullanici Kullanici { get; set; } = null!;
    public Bolum Bolum { get; set; } = null!;
    public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
}
