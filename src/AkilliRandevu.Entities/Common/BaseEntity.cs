namespace AkilliRandevu.Entities.Common;

/// <summary>
/// Tüm tabloların ortak alanları. Id her tablonun birincil anahtarıdır (PK).
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
}
