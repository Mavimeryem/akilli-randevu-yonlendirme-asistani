using AkilliRandevu.Business.Abstract;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.Business.Concrete;

public class RandevuService : IRandevuService
{
    private readonly IRandevuRepository _randevular;

    public RandevuService(IRandevuRepository randevular)
    {
        _randevular = randevular;
    }

    public Task<List<Randevu>> HastaGecmisiAsync(int hastaId) =>
        _randevular.GetHastaRandevulariAsync(hastaId);

    public Task<List<Randevu>> DoktorGunlukAsync(int doktorId, DateOnly gun) =>
        _randevular.GetDoktorGunlukRandevulariAsync(doktorId, gun);
}
