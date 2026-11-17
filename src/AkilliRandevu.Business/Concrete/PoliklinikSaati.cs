using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Takvim;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.Concrete;

public sealed class PoliklinikSaati : ISaat
{
    private readonly TimeProvider _zaman;
    private readonly TimeZoneInfo _dilim;

    public PoliklinikSaati(TimeProvider zaman, IOptions<TakvimAyarlari> ayarlar)
    {
        _zaman = zaman;
        _dilim = TimeZoneInfo.TryFindSystemTimeZoneById(ayarlar.Value.SaatDilimi, out var tz) ? tz : TimeZoneInfo.Local;
    }

    public DateTime UtcSimdi => _zaman.GetUtcNow().UtcDateTime;

    public DateTime Simdi => YereleCevir(UtcSimdi);

    public DateTime YereleCevir(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _dilim);
}
