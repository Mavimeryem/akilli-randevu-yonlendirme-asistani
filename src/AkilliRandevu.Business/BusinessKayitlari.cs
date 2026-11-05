using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Concrete;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.DataAccess;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.DataAccess.Context;
using Microsoft.Extensions.DependencyInjection;

namespace AkilliRandevu.Business;

/// <summary>
/// Web katmanı yalnızca bu sınıfı çağırır; veri katmanının ayrıntılarını bilmez.
/// </summary>
public static class BusinessKayitlari
{
    /// <summary>Geliştirme ortamında örnek kullanıcıların ortak şifresi.</summary>
    public const string DemoSifre = "Demo1234";

    public static IServiceCollection AddBusiness(this IServiceCollection services, string connectionString)
    {
        services.AddDataAccess(connectionString);

        services.AddOptions<TakvimAyarlari>();   // değerler Web katmanında appsettings'ten bağlanır
        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ISifreHasher, Pbkdf2SifreHasher>();
        services.AddSingleton<IGirisDenemeTakipcisi, BellekGirisDenemeTakipcisi>();
        services.AddScoped<IKullaniciService, KullaniciService>();
        services.AddScoped<IRandevuService, RandevuService>();
        services.AddScoped<IBelirtiService, BelirtiService>();
        return services;
    }

    public static Task VeritabaniniHazirlaAsync(this IServiceProvider services) =>
        services.VeritabaniniGuncelleAsync();

    /// <summary>
    /// Sentetik kullanıcıların yer tutucu şifresini <see cref="DemoSifre"/>'nin gerçek hash'iyle değiştirir.
    /// Her kullanıcı ayrı rastgele tuz alır. Yalnızca geliştirme ortamında çağrılmalıdır.
    /// </summary>
    public static async Task DemoSifreleriniAyarlaAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var kullanicilar = scope.ServiceProvider.GetRequiredService<IKullaniciRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<ISifreHasher>();

        foreach (var k in await kullanicilar.WhereAsync(k => k.SifreHash == SentetikVeri.SahteHash))
        {
            k.SifreHash = hasher.Hashle(DemoSifre);
            await kullanicilar.UpdateAsync(k);
        }
    }
}
