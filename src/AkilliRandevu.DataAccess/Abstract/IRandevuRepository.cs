using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.DataAccess.Abstract;

public interface IRandevuRepository : IRepository<Randevu>
{
    /// <summary>Hasta geçmişi: hastanın tüm randevuları, doktor ve bölüm bilgisiyle, yeniden eskiye.</summary>
    Task<List<Randevu>> GetHastaRandevulariAsync(int hastaId);

    /// <summary>Randevu takvimi: doktorun bir günkü randevuları (iptal edilenler hariç).</summary>
    Task<List<Randevu>> GetDoktorGunlukRandevulariAsync(int doktorId, DateOnly gun);

    /// <summary>Doktorun verilen aralıkta iptal edilmemiş başka randevusu var mı?</summary>
    Task<bool> CakismaVarMiAsync(int doktorId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null);
}
