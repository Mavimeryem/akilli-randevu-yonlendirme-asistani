using AkilliRandevu.Business;
using AkilliRandevu.Business.Bildirimler;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.Business.YapayZeka;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(o =>
{
    // Tüm POST isteklerinde anti-forgery token zorunlu (CSRF koruması).
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddBusiness(builder.Configuration.GetConnectionString("Varsayilan")!);
builder.Services.Configure<TakvimAyarlari>(builder.Configuration.GetSection("Takvim"));
// E-posta API anahtarı appsettings.json'da DEĞİL; ortam değişkeninden gelir: Eposta__ApiAnahtari
// (geliştirmede: dotnet user-secrets set "Eposta:ApiAnahtari" "...")
builder.Services.Configure<EpostaAyarlari>(builder.Configuration.GetSection("Eposta"));
builder.Services.Configure<BildirimAyarlari>(builder.Configuration.GetSection("Bildirim"));
// Claude API anahtarı da dosyada değil: ANTHROPIC_API_KEY ortam değişkeni
builder.Services.Configure<YapayZekaAyarlari>(builder.Configuration.GetSection("YapayZeka"));

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
if (app.Environment.IsDevelopment())
{
    await app.Services.DemoSifreleriniAyarlaAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization("tr-TR"); // tarih/gün adları Türkçe (ör. "Perşembe")
app.UseRouting();

app.UseAuthentication(); // "Kim bu?"  -> çerezden kullanıcıyı okur
app.UseAuthorization();  // "Yetkisi var mı?" -> [Authorize] kurallarını uygular

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

/// <summary>Entegrasyon testleri (WebApplicationFactory) uygulamayı bu sınıf üzerinden başlatır.</summary>
public partial class Program;
