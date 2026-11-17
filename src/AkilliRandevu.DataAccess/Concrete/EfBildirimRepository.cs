using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfBildirimRepository : EfRepository<Bildirim>, IBildirimRepository
{
    public EfBildirimRepository(AppDbContext context) : base(context)
    {
    }

    public Task<List<Bildirim>> GetKullaniciBildirimleriAsync(int kullaniciId, int adet) =>
        Set.AsNoTracking()
           .Include(b => b.EpostaGonderimleri)
           .Where(b => b.KullaniciId == kullaniciId)
           .OrderByDescending(b => b.OlusturmaTarihi).ThenByDescending(b => b.Id)
           .Take(adet)
           .ToListAsync();

    // IX_Bildirimler_KullaniciId_OkunduMu indeksini kullanır.
    public Task<int> OkunmamisSayisiAsync(int kullaniciId) =>
        Set.CountAsync(b => b.KullaniciId == kullaniciId && !b.OkunduMu);

    // UPDATE Bildirimler SET OkunduMu = 1 WHERE KullaniciId = @k AND OkunduMu = 0 [AND Id = @id]
    public Task OkunduYapAsync(int kullaniciId, int? bildirimId) =>
        Set.Where(b => b.KullaniciId == kullaniciId && !b.OkunduMu && (bildirimId == null || b.Id == bildirimId))
           .ExecuteUpdateAsync(s => s.SetProperty(b => b.OkunduMu, true));

    // IX_EpostaGonderimleri_Durum_OlusturmaTarihi indeksini kullanır.
    public Task<List<EpostaGonderimi>> GetBekleyenEpostalarAsync(int adet) =>
        Context.EpostaGonderimleri
               .Include(e => e.Bildirim)
               .Where(e => e.Durum == GonderimDurumu.Bekliyor)
               .OrderBy(e => e.OlusturmaTarihi).ThenBy(e => e.Id)
               .Take(adet)
               .ToListAsync();

    public async Task UpdateEpostaAsync(EpostaGonderimi gonderim)
    {
        if (Context.Entry(gonderim).State == EntityState.Detached)
            Context.EpostaGonderimleri.Update(gonderim);
        await Context.SaveChangesAsync();
    }

    public Task<List<EpostaGonderimi>> GetSonEpostalarAsync(int adet) =>
        Context.EpostaGonderimleri.AsNoTracking()
               .Include(e => e.Bildirim)
               .OrderByDescending(e => e.OlusturmaTarihi).ThenByDescending(e => e.Id)
               .Take(adet)
               .ToListAsync();
}
