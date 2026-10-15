using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfRandevuRepository : EfRepository<Randevu>, IRandevuRepository
{
    public EfRandevuRepository(AppDbContext context) : base(context)
    {
    }

    // Randevular JOIN Doktorlar JOIN Kullanicilar JOIN Bolumler WHERE HastaId = @id ORDER BY BaslangicZamani DESC
    // IX_Randevular_HastaId_BaslangicZamani indeksini kullanır.
    public Task<List<Randevu>> GetHastaRandevulariAsync(int hastaId) =>
        Set.AsNoTracking()
           .Include(r => r.Doktor).ThenInclude(d => d.Kullanici)
           .Include(r => r.Doktor).ThenInclude(d => d.Bolum)
           .Where(r => r.HastaId == hastaId)
           .OrderByDescending(r => r.BaslangicZamani)
           .ToListAsync();

    // IX_Randevular_DoktorId_BaslangicZamani indeksini kullanır.
    public Task<List<Randevu>> GetDoktorGunlukRandevulariAsync(int doktorId, DateOnly gun)
    {
        var gunBasi = gun.ToDateTime(TimeOnly.MinValue);
        var gunSonu = gunBasi.AddDays(1);
        return Set.AsNoTracking()
                  .Include(r => r.Hasta).ThenInclude(h => h.Kullanici)
                  .Where(r => r.DoktorId == doktorId
                           && r.BaslangicZamani >= gunBasi
                           && r.BaslangicZamani < gunSonu
                           && r.Durum != RandevuDurumu.IptalEdildi)
                  .OrderBy(r => r.BaslangicZamani)
                  .ToListAsync();
    }

    // İki aralık çakışır: mevcut.Baslangic < yeni.Bitis VE mevcut.Bitis > yeni.Baslangic
    public Task<bool> CakismaVarMiAsync(int doktorId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null) =>
        Set.AnyAsync(r => r.DoktorId == doktorId
                       && r.Durum != RandevuDurumu.IptalEdildi
                       && r.Id != haricRandevuId
                       && r.BaslangicZamani < bitis
                       && r.BitisZamani > baslangic);
}
