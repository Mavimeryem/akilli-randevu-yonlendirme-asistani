using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.Business.Abstract;

public interface IRandevuService
{
    Task<List<Randevu>> HastaGecmisiAsync(int hastaId);

    Task<List<Randevu>> DoktorGunlukAsync(int doktorId, DateOnly gun);
}
