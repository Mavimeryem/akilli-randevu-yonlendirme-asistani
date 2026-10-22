using AkilliRandevu.Business;
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
app.UseRouting();

app.UseAuthentication(); // "Kim bu?"  -> çerezden kullanıcıyı okur
app.UseAuthorization();  // "Yetkisi var mı?" -> [Authorize] kurallarını uygular

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
