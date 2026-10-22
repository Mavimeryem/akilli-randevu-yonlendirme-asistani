using System.Security.Claims;
using AkilliRandevu.Entities.Concrete;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AkilliRandevu.Web.Infrastructure;

/// <summary>
/// Oturum çerezine yazılan kullanıcı bilgileri (claim). Çerez sunucu tarafından şifrelenir ve
/// imzalanır; kullanıcı içeriğini okuyamaz ya da rolünü değiştiremez. Şifre/hash asla eklenmez.
/// </summary>
public static class KullaniciClaimleri
{
    public const string HastaId = "hasta_id";
    public const string DoktorId = "doktor_id";

    public static ClaimsPrincipal Olustur(Kullanici k)
    {
        var claimler = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, k.Id.ToString()),
            new(ClaimTypes.Name, $"{k.Ad} {k.Soyad}"),
            new(ClaimTypes.Email, k.Eposta),
            new(ClaimTypes.Role, k.Rol.ToString())
        };
        if (k.Hasta is not null)
            claimler.Add(new Claim(HastaId, k.Hasta.Id.ToString()));
        if (k.Doktor is not null)
            claimler.Add(new Claim(DoktorId, k.Doktor.Id.ToString()));

        var kimlik = new ClaimsIdentity(claimler, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(kimlik);
    }

    public static int? GetHastaId(this ClaimsPrincipal u) => SayiOku(u, HastaId);

    public static int? GetDoktorId(this ClaimsPrincipal u) => SayiOku(u, DoktorId);

    private static int? SayiOku(ClaimsPrincipal u, string tur) =>
        int.TryParse(u.FindFirstValue(tur), out var id) ? id : null;
}
