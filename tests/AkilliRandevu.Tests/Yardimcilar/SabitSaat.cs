using AkilliRandevu.Business.Abstract;

namespace AkilliRandevu.Tests.Yardimcilar;

/// <summary>Testlerde "şimdi"yi sabitlemek için. Poliklinik yerel saati = UTC + 3 (Europe/Istanbul).</summary>
public sealed class SabitSaat : ISaat
{
    public SabitSaat(DateTime yerelSimdi)
    {
        Simdi = yerelSimdi;
    }

    public DateTime Simdi { get; set; }

    public DateTime UtcSimdi => DateTime.SpecifyKind(Simdi.AddHours(-3), DateTimeKind.Utc);

    public DateTime YereleCevir(DateTime utc) => utc.AddHours(3);
}
