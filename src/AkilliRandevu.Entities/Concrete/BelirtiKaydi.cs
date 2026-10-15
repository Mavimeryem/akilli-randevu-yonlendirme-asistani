using AkilliRandevu.Entities.Common;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Hastanın belirti girişi ve sistemin önerdiği bölüm.
/// Hasta 1-N BelirtiKaydi, BelirtiKaydi N-N Belirti (BelirtiKaydiBelirti üzerinden).
/// </summary>
public class BelirtiKaydi : BaseEntity
{
    public int HastaId { get; set; }
    public int? OnerilenBolumId { get; set; }
    public string? Aciklama { get; set; }
    public AciliyetSeviyesi Aciliyet { get; set; } = AciliyetSeviyesi.Dusuk;

    public Hasta Hasta { get; set; } = null!;
    public Bolum? OnerilenBolum { get; set; }
    public Randevu? Randevu { get; set; }
    public ICollection<BelirtiKaydiBelirti> Belirtiler { get; set; } = new List<BelirtiKaydiBelirti>();
}
