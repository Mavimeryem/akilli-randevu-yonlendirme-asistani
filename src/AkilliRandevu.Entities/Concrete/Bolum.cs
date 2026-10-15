using AkilliRandevu.Entities.Common;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Poliklinik bölümü (Kardiyoloji, Dahiliye vb.).
/// Bolum N-N Belirti ilişkisi BelirtiBolum ara tablosuyla kurulur.
/// </summary>
public class Bolum : BaseEntity
{
    public string Ad { get; set; } = null!;
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; } = true;

    public ICollection<Doktor> Doktorlar { get; set; } = new List<Doktor>();
    public ICollection<BelirtiBolum> BelirtiBolumleri { get; set; } = new List<BelirtiBolum>();
}
