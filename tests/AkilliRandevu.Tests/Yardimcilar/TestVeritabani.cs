using AkilliRandevu.DataAccess.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.Tests.Yardimcilar;

/// <summary>
/// Her test için bellekte (in-memory) SQLite veritabanı. Şema EnsureCreated ile değil migration'larla
/// kurulur; böylece elle yazılan migration'lar ve sentetik veri de her test çalıştırmasında doğrulanır.
/// </summary>
public sealed class TestVeritabani : IDisposable
{
    private readonly SqliteConnection _baglanti;

    public TestVeritabani()
    {
        _baglanti = new SqliteConnection("DataSource=:memory:");
        _baglanti.Open(); // bağlantı açık kaldıkça bellekteki veritabanı yaşar

        using var db = YeniContext();
        db.Database.Migrate();
    }

    public DbContextOptions<AppDbContext> Secenekler =>
        new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_baglanti).Options;

    /// <summary>Aynı veritabanına bağlı yeni bir context (gerçek uygulamadaki "istek başına context" gibi).</summary>
    public AppDbContext YeniContext() => new(Secenekler);

    public void Dispose() => _baglanti.Dispose();
}
