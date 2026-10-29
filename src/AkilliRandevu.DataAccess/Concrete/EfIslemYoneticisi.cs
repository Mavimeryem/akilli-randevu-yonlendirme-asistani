using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;

namespace AkilliRandevu.DataAccess.Concrete;

public class EfIslemYoneticisi : IIslemYoneticisi
{
    private readonly AppDbContext _context;

    public EfIslemYoneticisi(AppDbContext context)
    {
        _context = context;
    }

    // SQLite'ta BeginTransaction "BEGIN IMMEDIATE" çalıştırır: yazma kilidi hemen alınır.
    // Böylece "kontrol et + ekle" adımları arasında başka bir istek araya giremez
    // (aynı slota aynı anda iki randevu yazılamaz).
    public async Task<T> TransactionIcindeAsync<T>(Func<Task<T>> islem)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        var sonuc = await islem();
        await tx.CommitAsync();
        return sonuc;
    }
}
