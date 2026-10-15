namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Belirti N-N Bolum ara tablosu. Birleşik birincil anahtar: (BelirtiId, BolumId).
/// AgirlikPuani, belirtinin o bölüme ne kadar işaret ettiğini gösterir (yönlendirmede kullanılır).
/// </summary>
public class BelirtiBolum
{
    public int BelirtiId { get; set; }
    public int BolumId { get; set; }
    public int AgirlikPuani { get; set; }

    public Belirti Belirti { get; set; } = null!;
    public Bolum Bolum { get; set; } = null!;
}
