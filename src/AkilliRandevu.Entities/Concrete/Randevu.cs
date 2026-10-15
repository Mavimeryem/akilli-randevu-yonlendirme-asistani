using AkilliRandevu.Entities.Common;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Hasta ile doktor arasındaki randevu.
/// Bölüm bilgisi doktordan türetildiği için burada ayrıca tutulmaz (3NF).
/// BelirtiKaydi 1-0..1 Randevu: randevu bir belirti kaydından doğmuş olabilir.
/// </summary>
public class Randevu : BaseEntity
{
    public int HastaId { get; set; }
    public int DoktorId { get; set; }
    public int? BelirtiKaydiId { get; set; }
    public DateTime BaslangicZamani { get; set; }
    public DateTime BitisZamani { get; set; }
    public RandevuDurumu Durum { get; set; } = RandevuDurumu.Beklemede;
    public string? DoktorNotu { get; set; }

    public Hasta Hasta { get; set; } = null!;
    public Doktor Doktor { get; set; } = null!;
    public BelirtiKaydi? BelirtiKaydi { get; set; }
}
