using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfBolumRepository : EfRepository<Bolum>, IBolumRepository
{
    public EfBolumRepository(AppDbContext context) : base(context)
    {
    }

    public Task<List<Bolum>> GetDoktorlarIleAsync() =>
        Set.AsNoTracking()
           .Include(b => b.Doktorlar).ThenInclude(d => d.Kullanici)
           .Where(b => b.AktifMi)
           .OrderBy(b => b.Ad)
           .ToListAsync();

    // SELECT b.Id, b.Ad, SUM(bb.AgirlikPuani) FROM BelirtiBolumleri bb JOIN Bolumler b ...
    // WHERE bb.BelirtiId IN (...) AND b.AktifMi GROUP BY b.Id, b.Ad ORDER BY 3 DESC
    public async Task<List<BolumPuani>> BelirtilereGorePuanlaAsync(IEnumerable<int> belirtiIdleri)
    {
        var idler = belirtiIdleri.Distinct().ToList();
        return await Context.BelirtiBolumleri
            .Where(bb => idler.Contains(bb.BelirtiId) && bb.Bolum.AktifMi)
            .GroupBy(bb => new { bb.BolumId, bb.Bolum.Ad })
            .Select(g => new BolumPuani(g.Key.BolumId, g.Key.Ad, g.Sum(x => x.AgirlikPuani)))
            .OrderByDescending(x => x.ToplamPuan)
            .ToListAsync();
    }
}
