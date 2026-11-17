using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Bildirimler;
using AkilliRandevu.Business.Concrete;
using AkilliRandevu.Business.Gecmis;
using AkilliRandevu.Business.Istatistik;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.Business.YapayZeka;
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

        // Ayar değerleri Web katmanında appsettings / ortam değişkenlerinden bağlanır.
        services.AddOptions<TakvimAyarlari>();
        services.AddOptions<EpostaAyarlari>();
        services.AddOptions<BildirimAyarlari>();
        services.AddOptions<YapayZekaAyarlari>();

        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ISaat, PoliklinikSaati>();
        services.AddSingleton<ISifreHasher, Pbkdf2SifreHasher>();
        services.AddSingleton<IGirisDenemeTakipcisi, BellekGirisDenemeTakipcisi>();
        services.AddScoped<IKullaniciService, KullaniciService>();
        services.AddScoped<IRandevuService, RandevuService>();
        services.AddScoped<IBelirtiService, BelirtiService>();
        services.AddScoped<IHastaGecmisiService, HastaGecmisiService>();
        services.AddScoped<IIstatistikService, IstatistikService>();

        // Bildirim: uygulama içi + e-posta giden kutusu + hatırlatma zamanlayıcısı
        services.AddHttpClient<IEpostaGonderici, BrevoEpostaGonderici>(c =>
            c.Timeout = TimeSpan.FromSeconds(60)); // her denemenin kendi zaman aşımı ayrıca var (EpostaAyarlari)
        services.AddScoped<IBildirimService, BildirimService>();
        services.AddScoped<IHatirlatmaServisi, HatirlatmaServisi>();
        services.AddScoped<IEpostaKuyrugu, EpostaKuyrugu>();
        services.AddHostedService<BildirimArkaPlanServisi>();

        // Yapay zekâ: belirti metninden bölüm önerisi (Claude). İstemci tekil: SDK istemcisi yeniden kullanılır.
        services.AddSingleton<IYapayZekaIstemcisi, ClaudeYapayZekaIstemcisi>();
        services.AddSingleton<IBolumOneriAsistani, BolumOneriAsistani>();
        return services;
    }

    public static Task VeritabaniniHazirlaAsync(this IServiceProvider services) =>
        services.VeritabaniniGuncelleAsync();

    /// <summary>
    /// Gerçek kullanım için ilk yönetici hesabı. Bu e-postayla kullanıcı yoksa yönetici olarak oluşturulur;
    /// varsa dokunulmaz (her açılışta şifre sıfırlanmaz). Şifre kuralına uymuyorsa uygulama başlamaz.
    /// </summary>
    public static async Task IlkYoneticiyiHazirlaAsync(this IServiceProvider services, string eposta, string sifre)
    {
        using var scope = services.CreateScope();
        var kullanicilar = scope.ServiceProvider.GetRequiredService<IKullaniciRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<ISifreHasher>();

        eposta = KullaniciService.EpostaNormallestir(eposta);
        if (await kullanicilar.AnyAsync(k => k.Eposta == eposta))
            return;
        if (sifre.Length < KullaniciService.MinimumSifreUzunlugu || !sifre.Any(char.IsLetter) || !sifre.Any(char.IsDigit))
            throw new InvalidOperationException(
                $"IlkYonetici__Sifre en az {KullaniciService.MinimumSifreUzunlugu} karakter olmalı ve harf ile rakam içermelidir.");

        await kullanicilar.AddAsync(new Entities.Concrete.Kullanici
        {
            Ad = "Sistem",
            Soyad = "Yöneticisi",
            Eposta = eposta,
            SifreHash = hasher.Hashle(sifre),
            Rol = Entities.Enums.Rol.Yonetici
        });
    }

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
