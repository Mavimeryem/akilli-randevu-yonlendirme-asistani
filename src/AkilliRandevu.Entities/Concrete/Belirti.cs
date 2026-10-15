using AkilliRandevu.Entities.Common;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Sistemde tanımlı belirti kataloğu (baş ağrısı, göğüs ağrısı vb.).
/// </summary>
public class Belirti : BaseEntity
{
    public string Ad { get; set; } = null!;
    public string? Aciklama { get; set; }

    public ICollection<BelirtiBolum> BelirtiBolumleri { get; set; } = new List<BelirtiBolum>();
    public ICollection<BelirtiKaydiBelirti> BelirtiKaydiBelirtileri { get; set; } = new List<BelirtiKaydiBelirti>();
}
