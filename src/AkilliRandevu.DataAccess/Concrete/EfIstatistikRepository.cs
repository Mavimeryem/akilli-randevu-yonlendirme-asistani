using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfIstatistikRepository : IIstatistikRepository
{
    private readonly AppDbContext _db;

    public EfIstatistikRepository(AppDbContext db)
    {
        _db = db;
    }

    // Projeksiyon: SELECT r.Id, r.BaslangicZamani, ..., b.Ad, k.Ad ... (varlıkların tamamı yüklenmez)
    public Task<List<RandevuOzeti>> GetRandevularAsync(DateTime baslangic, DateTime bitis, int? bolumId)
    {
        var q = _db.Randevular.AsNoTracking()
            .Where(r => r.BaslangicZamani >= baslangic && r.BaslangicZamani < bitis);
        if (bolumId is { } b)
            q = q.Where(r => r.Doktor.BolumId == b);

        return q.OrderBy(r => r.BaslangicZamani)
            .Select(r => new RandevuOzeti(
                r.Id,
                r.BaslangicZamani,
                r.BitisZamani,
                r.Durum,
                r.DoktorId,
                r.Doktor.Unvan + " " + r.Doktor.Kullanici.Ad + " " + r.Doktor.Kullanici.Soyad,
                r.Doktor.BolumId,
                r.Doktor.Bolum.Ad,
                r.Hasta.Kullanici.Ad,
                r.Hasta.Kullanici.Soyad))
            .ToListAsync();
    }

    public Task<List<DoktorOzeti>> GetAktifDoktorlarAsync(int? bolumId)
    {
        var q = _db.Doktorlar.AsNoTracking().Where(d => d.Kullanici.AktifMi && d.Bolum.AktifMi);
        if (bolumId is { } b)
            q = q.Where(d => d.BolumId == b);

        return q.OrderBy(d => d.Bolum.Ad).ThenBy(d => d.Kullanici.Ad)
            .Select(d => new DoktorOzeti(d.Id, d.Unvan + " " + d.Kullanici.Ad + " " + d.Kullanici.Soyad, d.BolumId, d.Bolum.Ad))
            .ToListAsync();
    }
}
