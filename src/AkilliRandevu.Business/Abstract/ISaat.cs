namespace AkilliRandevu.Business.Abstract;

/// <summary>
/// Poliklinik saati. Randevu saatleri poliklinik yerel saatiyle (Europe/Istanbul) tutulur; sunucunun
/// saat dilimi ne olursa olsun (bulut sunucular genelde UTC'dir) "şimdi" buradan alınır.
/// </summary>
public interface ISaat
{
    DateTime UtcSimdi { get; }

    /// <summary>Poliklinik yerel saati.</summary>
    DateTime Simdi { get; }

    /// <summary>UTC zamanı poliklinik yerel saatine çevirir (ör. OlusturmaTarihi göstermek için).</summary>
    DateTime YereleCevir(DateTime utc);
}
