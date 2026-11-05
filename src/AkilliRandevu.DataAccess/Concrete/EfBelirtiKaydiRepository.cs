using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfBelirtiKaydiRepository : EfRepository<BelirtiKaydi>, IBelirtiKaydiRepository
{
    public EfBelirtiKaydiRepository(AppDbContext context) : base(context)
    {
    }

    public Task<BelirtiKaydi?> GetDetayliAsync(int kayitId) =>
        Set.AsNoTracking()
           .Include(k => k.Belirtiler).ThenInclude(b => b.Belirti)
           .Include(k => k.OnerilenBolum)
           .Include(k => k.Randevu)
           .FirstOrDefaultAsync(k => k.Id == kayitId);

    // IX_BelirtiKayitlari_HastaId_OlusturmaTarihi indeksini kullanır.
    public Task<List<BelirtiKaydi>> GetHastaKayitlariAsync(int hastaId) =>
        Set.AsNoTracking()
           .Include(k => k.Belirtiler).ThenInclude(b => b.Belirti)
           .Include(k => k.OnerilenBolum)
           .Include(k => k.Randevu)
           .Where(k => k.HastaId == hastaId)
           .OrderByDescending(k => k.OlusturmaTarihi)
           .ToListAsync();

    public Task<int> SayAsync(int hastaId, DateTime baslangic) =>
        Set.CountAsync(k => k.HastaId == hastaId && k.OlusturmaTarihi >= baslangic);
}
