using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfKullaniciRepository : EfRepository<Kullanici>, IKullaniciRepository
{
    public EfKullaniciRepository(AppDbContext context) : base(context)
    {
    }

    // IX_Kullanicilar_Eposta (UNIQUE) indeksini kullanır.
    public Task<Kullanici?> GetByEpostaAsync(string eposta) =>
        Set.AsNoTracking()
           .Include(k => k.Hasta)
           .Include(k => k.Doktor).ThenInclude(d => d!.Bolum)
           .FirstOrDefaultAsync(k => k.Eposta == eposta);

    public Task<List<Kullanici>> GetTumuProfilleriyleAsync() =>
        Set.AsNoTracking()
           .Include(k => k.Doktor).ThenInclude(d => d!.Bolum)
           .OrderBy(k => k.Rol).ThenBy(k => k.Ad).ThenBy(k => k.Soyad)
           .ToListAsync();
}
