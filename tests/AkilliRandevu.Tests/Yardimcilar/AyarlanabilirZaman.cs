namespace AkilliRandevu.Tests.Yardimcilar;

/// <summary>Testte zamanı elle ileri almak için TimeProvider (ör. "15 dakika sonra kilit açılıyor mu?").</summary>
public sealed class AyarlanabilirZaman : TimeProvider
{
    public AyarlanabilirZaman(DateTimeOffset utcSimdi)
    {
        UtcSimdi = utcSimdi;
    }

    public DateTimeOffset UtcSimdi { get; set; }

    public override DateTimeOffset GetUtcNow() => UtcSimdi;

    public void IleriAl(TimeSpan sure) => UtcSimdi += sure;
}
