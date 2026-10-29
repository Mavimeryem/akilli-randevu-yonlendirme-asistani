using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.DataAccess.Abstract;

public interface IRandevuRepository : IRepository<Randevu>
{
    /// <summary>Hasta geçmişi: hastanın tüm randevuları, doktor ve bölüm bilgisiyle, yeniden eskiye.</summary>
    Task<List<Randevu>> GetHastaRandevulariAsync(int hastaId);

    /// <summary>Randevu takvimi: doktorun bir günkü randevuları (iptal edilenler hariç).</summary>
    Task<List<Randevu>> GetDoktorGunlukRandevulariAsync(int doktorId, DateOnly gun);

    /// <summary>Yönetici ekranı: bir günün tüm randevuları (iptaller dahil), isteğe bağlı bölüm filtresi.</summary>
    Task<List<Randevu>> GetGunlukTumuAsync(DateOnly gun, int? bolumId);

    /// <summary>Durum değişikliği için: randevu, hasta/doktor/bölüm bilgisiyle ve değişiklik takibi açık.</summary>
    Task<Randevu?> GetDetayliAsync(int randevuId);

    /// <summary>Doktorun verilen aralıkta iptal edilmemiş başka randevusu var mı?</summary>
    Task<bool> CakismaVarMiAsync(int doktorId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null);

    /// <summary>Hastanın aynı saatte (başka doktorda bile) iptal edilmemiş randevusu var mı?</summary>
    Task<bool> HastaCakismaVarMiAsync(int hastaId, DateTime baslangic, DateTime bitis);

    /// <summary>Hastanın bu bölümde ileri tarihli, aktif (beklemede/onaylı) randevusu var mı?</summary>
    Task<bool> BolumdeAktifRandevuVarMiAsync(int hastaId, int bolumId, DateTime simdi);
}
