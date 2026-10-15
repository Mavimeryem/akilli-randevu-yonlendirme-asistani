using AkilliRandevu.Entities.Common;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Hastaya özgü bilgiler. Hasta 1-N Randevu, Hasta 1-N BelirtiKaydi.
/// Randevular ve belirti kayıtları birlikte hasta geçmişini oluşturur.
/// </summary>
public class Hasta : BaseEntity
{
    public int KullaniciId { get; set; }
    public string TcKimlikNo { get; set; } = null!;
    public DateOnly DogumTarihi { get; set; }
    public Cinsiyet Cinsiyet { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
    public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    public ICollection<BelirtiKaydi> BelirtiKayitlari { get; set; } = new List<BelirtiKaydi>();
}
