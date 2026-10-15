using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Concrete;
using AkilliRandevu.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AkilliRandevu.DataAccess;

public static class DataAccessKayitlari
{
    /// <summary>DbContext ve repository'leri dependency injection'a kaydeder.</summary>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IRandevuRepository, EfRandevuRepository>();
        services.AddScoped<IBolumRepository, EfBolumRepository>();
        return services;
    }

    /// <summary>Uygulanmamış migration'ları veritabanına uygular (yoksa veritabanını oluşturur).</summary>
    public static async Task VeritabaniniGuncelleAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
