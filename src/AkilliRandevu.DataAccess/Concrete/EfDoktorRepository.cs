using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfDoktorRepository : EfRepository<Doktor>, IDoktorRepository
{
    public EfDoktorRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Doktor?> GetDetayliAsync(int doktorId) =>
        Set.AsNoTracking()
           .Include(d => d.Kullanici)
           .Include(d => d.Bolum)
           .FirstOrDefaultAsync(d => d.Id == doktorId);

    public Task<List<Doktor>> GetBolumdekilerAsync(int bolumId) =>
        Set.AsNoTracking()
           .Include(d => d.Kullanici)
           .Where(d => d.BolumId == bolumId && d.Kullanici.AktifMi)
           .OrderBy(d => d.Kullanici.Ad).ThenBy(d => d.Kullanici.Soyad)
           .ToListAsync();
}
