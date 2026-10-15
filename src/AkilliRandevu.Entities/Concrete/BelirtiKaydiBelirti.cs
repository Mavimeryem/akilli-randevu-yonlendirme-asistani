namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// BelirtiKaydi N-N Belirti ara tablosu. Birleşik birincil anahtar: (BelirtiKaydiId, BelirtiId).
/// </summary>
public class BelirtiKaydiBelirti
{
    public int BelirtiKaydiId { get; set; }
    public int BelirtiId { get; set; }
    /// <summary>1-10 arası şiddet.</summary>
    public int Siddet { get; set; }

    public BelirtiKaydi BelirtiKaydi { get; set; } = null!;
    public Belirti Belirti { get; set; } = null!;
}
