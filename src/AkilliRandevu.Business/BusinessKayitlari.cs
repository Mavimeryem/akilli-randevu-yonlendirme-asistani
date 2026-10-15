using AkilliRandevu.DataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace AkilliRandevu.Business;

/// <summary>
/// Web katmanı yalnızca bu sınıfı çağırır; veri katmanının ayrıntılarını bilmez.
/// İş servisleri ilerleyen haftalarda buraya kaydedilecek.
/// </summary>
public static class BusinessKayitlari
{
    public static IServiceCollection AddBusiness(this IServiceCollection services, string connectionString)
    {
        services.AddDataAccess(connectionString);
        return services;
    }

    public static Task VeritabaniniHazirlaAsync(this IServiceProvider services) =>
        services.VeritabaniniGuncelleAsync();
}
