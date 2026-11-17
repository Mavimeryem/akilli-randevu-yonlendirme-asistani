using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfHastaGecmisiRepository : IHastaGecmisiRepository
{
    private readonly AppDbContext _db;

    public EfHastaGecmisiRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Hasta?> GetHastaAsync(int hastaId) =>
        _db.Hastalar.AsNoTracking().Include(h => h.Kullanici).FirstOrDefaultAsync(h => h.Id == hastaId);

    public Task<bool> DoktorHastaIliskisiVarMiAsync(int doktorId, int hastaId) =>
        _db.Randevular.AnyAsync(r => r.DoktorId == doktorId && r.HastaId == hastaId);

    // IX_Randevular_HastaId_BaslangicZamani indeksini kullanır.
    public Task<List<Randevu>> GetRandevularAsync(int hastaId, DateTime? baslangic, DateTime? bitis, RandevuDurumu? durum, int? bolumId)
    {
        var q = _db.Randevular.AsNoTracking()
            .Include(r => r.Doktor).ThenInclude(d => d.Kullanici)
            .Include(r => r.Doktor).ThenInclude(d => d.Bolum)
            .Where(r => r.HastaId == hastaId);

        if (baslangic is { } b) q = q.Where(r => r.BaslangicZamani >= b);
        if (bitis is { } s) q = q.Where(r => r.BaslangicZamani < s);
        if (durum is { } d) q = q.Where(r => r.Durum == d);
        if (bolumId is { } bl) q = q.Where(r => r.Doktor.BolumId == bl);

        return q.OrderByDescending(r => r.BaslangicZamani).ToListAsync();
    }

    // IX_BelirtiKayitlari_HastaId_OlusturmaTarihi indeksini kullanır.
    public Task<List<BelirtiKaydi>> GetBelirtiKayitlariAsync(int hastaId, DateTime? baslangicUtc, DateTime? bitisUtc, int? bolumId)
    {
        var q = _db.BelirtiKayitlari.AsNoTracking()
            .Include(k => k.Belirtiler).ThenInclude(b => b.Belirti)
            .Include(k => k.OnerilenBolum)
            .Include(k => k.YzDegerlendirmesi).ThenInclude(y => y!.OnerilenBolum)
            .Where(k => k.HastaId == hastaId);

        if (baslangicUtc is { } b) q = q.Where(k => k.OlusturmaTarihi >= b);
        if (bitisUtc is { } s) q = q.Where(k => k.OlusturmaTarihi < s);
        if (bolumId is { } bl) q = q.Where(k => k.OnerilenBolumId == bl);

        return q.OrderByDescending(k => k.OlusturmaTarihi).ToListAsync();
    }

    // SELECT HastaId, MAX(BaslangicZamani), COUNT(*) FROM Randevular WHERE DoktorId = @d GROUP BY HastaId
    public async Task<List<DoktorHastasi>> GetDoktorHastalariAsync(int doktorId)
    {
        var gruplar = await _db.Randevular.AsNoTracking()
            .Where(r => r.DoktorId == doktorId)
            .GroupBy(r => r.HastaId)
            .Select(g => new { HastaId = g.Key, Son = g.Max(r => r.BaslangicZamani), Sayi = g.Count() })
            .ToListAsync();

        var idler = gruplar.Select(g => g.HastaId).ToList();
        var hastalar = await _db.Hastalar.AsNoTracking().Include(h => h.Kullanici)
            .Where(h => idler.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id);

        return gruplar
            .Select(g => new DoktorHastasi(g.HastaId, hastalar[g.HastaId].Kullanici.Ad, hastalar[g.HastaId].Kullanici.Soyad, g.Son, g.Sayi))
            .OrderByDescending(h => h.SonRandevu)
            .ToList();
    }

    public async Task ErisimKaydetAsync(ErisimKaydi kayit)
    {
        _db.ErisimKayitlari.Add(kayit);
        await _db.SaveChangesAsync();
    }

    // IX_ErisimKayitlari_HastaId_OlusturmaTarihi indeksini kullanır.
    public Task<List<ErisimKaydi>> GetErisimKayitlariAsync(int? hastaId, int adet)
    {
        var q = _db.ErisimKayitlari.AsNoTracking()
            .Include(e => e.Erisen)
            .Include(e => e.Hasta).ThenInclude(h => h.Kullanici)
            .AsQueryable();
        if (hastaId is { } h) q = q.Where(e => e.HastaId == h);
        return q.OrderByDescending(e => e.OlusturmaTarihi).ThenByDescending(e => e.Id).Take(adet).ToListAsync();
    }
}
