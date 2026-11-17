using AkilliRandevu.Business;
using AkilliRandevu.Business.Bildirimler;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.Business.YapayZeka;
using AkilliRandevu.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var ayar = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews(o =>
{
    // Tüm POST isteklerinde anti-forgery token zorunlu (CSRF koruması).
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddBusiness(ayar.GetConnectionString("Varsayilan")!);
builder.Services.Configure<TakvimAyarlari>(ayar.GetSection("Takvim"));
// E-posta API anahtarı appsettings.json'da DEĞİL; ortam değişkeninden gelir: Eposta__ApiAnahtari
// (geliştirmede: dotnet user-secrets set "Eposta:ApiAnahtari" "...")
builder.Services.Configure<EpostaAyarlari>(ayar.GetSection("Eposta"));
builder.Services.Configure<BildirimAyarlari>(ayar.GetSection("Bildirim"));
// Claude API anahtarı da dosyada değil: ANTHROPIC_API_KEY ortam değişkeni
builder.Services.Configure<YapayZekaAyarlari>(ayar.GetSection("YapayZeka"));

// Demo modu: sentetik hesaplar (Demo1234) yalnızca geliştirmede ya da DemoModu=true iken çalışır.
var uygulama = new UygulamaBilgisi(DemoModu: builder.Environment.IsDevelopment() || ayar.GetValue<bool>("DemoModu"));
builder.Services.AddSingleton(uygulama);

// Oturum çerezlerini şifreleyen anahtarlar kalıcı bir klasörde tutulur; yoksa her yeniden başlatmada
// anahtar değişir ve herkesin oturumu düşer.
if (ayar["VeriKoruma:AnahtarKlasoru"] is { Length: > 0 } anahtarKlasoru)
{
    builder.Services.AddDataProtection()
        .SetApplicationName("AkilliRandevu")
        .PersistKeysToFileSystem(new DirectoryInfo(anahtarKlasoru));
}

// Bulut platformları (Render vb.) HTTPS'i kendi önlerindeki proxy'de sonlandırır ve isteği HTTP olarak iletir.
// Asıl şemayı X-Forwarded-Proto başlığından okumazsak HTTPS yönlendirmesi sonsuz döngüye girer.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    if (ayar.GetValue<bool>("ProxyArkasinda"))
    {
        o.KnownIPNetworks.Clear();
        o.KnownProxies.Clear();
    }
});

builder.Services.AddHealthChecks();

// Oturum: çerez tabanlı kimlik doğrulama.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = ".AkilliRandevu.Oturum";
        o.Cookie.HttpOnly = true;                       // JavaScript çereze erişemez (XSS ile çalınamaz)
        o.Cookie.SameSite = SameSiteMode.Lax;           // başka sitelerden gelen POST'larda gönderilmez
        o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;                // canlıda yalnızca HTTPS
        o.ExpireTimeSpan = TimeSpan.FromMinutes(30);    // 30 dk işlem yapılmazsa oturum düşer
        o.SlidingExpiration = true;                     // her istekte süre yenilenir
        o.LoginPath = "/Hesap/Giris";                   // giriş yapmamış kullanıcı buraya yönlenir
        o.AccessDeniedPath = "/Hesap/ErisimEngellendi"; // rolü yetmeyen kullanıcı buraya yönlenir
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Bekleyen migration'ları uygula; veritabanı yoksa örnek veriyle oluşturulur.
await app.Services.VeritabaniniHazirlaAsync();
if (uygulama.DemoModu)
{
    await app.Services.DemoSifreleriniAyarlaAsync();
}
// Gerçek kullanım için ilk yönetici hesabı (ortam değişkenleri: IlkYonetici__Eposta, IlkYonetici__Sifre).
if (ayar["IlkYonetici:Eposta"] is { Length: > 0 } yoneticiEposta && ayar["IlkYonetici:Sifre"] is { Length: > 0 } yoneticiSifre)
{
    await app.Services.IlkYoneticiyiHazirlaAsync(yoneticiEposta, yoneticiSifre);
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/Durum/{0}"); // 404 vb. için Türkçe hata sayfası

app.UseHttpsRedirection();
app.UseRequestLocalization("tr-TR"); // tarih/gün adları Türkçe (ör. "Perşembe")
app.UseRouting();

app.UseAuthentication(); // "Kim bu?"  -> çerezden kullanıcıyı okur
app.UseAuthorization();  // "Yetkisi var mı?" -> [Authorize] kurallarını uygular

app.MapStaticAssets();
app.MapHealthChecks("/saglik"); // platformun "uygulama ayakta mı?" kontrolü

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

/// <summary>Entegrasyon testleri (WebApplicationFactory) uygulamayı bu sınıf üzerinden başlatır.</summary>
public partial class Program;
