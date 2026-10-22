using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

public class HesapController : Controller
{
    private readonly IKullaniciService _kullaniciService;

    public HesapController(IKullaniciService kullaniciService)
    {
        _kullaniciService = kullaniciService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Giris(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new GirisViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Giris(GirisViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var sonuc = await _kullaniciService.GirisYapAsync(model.Eposta, model.Sifre);
        switch (sonuc.Durum)
        {
            case GirisDurumu.Basarili:
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    KullaniciClaimleri.Olustur(sonuc.Kullanici!));

                // Açık yönlendirme (open redirect) koruması: yalnızca site içi adreslere dönülür.
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return LocalRedirect(model.ReturnUrl);
                return RedirectToAction("Index", "Home");

            case GirisDurumu.Kilitli:
                var dk = (int)Math.Ceiling(sonuc.KalanKilitSuresi!.Value.TotalMinutes);
                ModelState.AddModelError("", $"Çok fazla başarısız deneme. {dk} dakika sonra tekrar deneyin.");
                break;

            case GirisDurumu.HesapPasif:
                ModelState.AddModelError("", "Hesabınız pasif durumda. Yönetici ile iletişime geçin.");
                break;

            default:
                // E-postanın mı şifrenin mi yanlış olduğu söylenmez.
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                break;
        }

        model.Sifre = "";
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Kayit()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new KayitViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Kayit(KayitViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var sonuc = await _kullaniciService.HastaKaydetAsync(new KayitIstegi(
            model.Ad, model.Soyad, model.Eposta, model.Sifre, model.Telefon,
            model.TcKimlikNo, DateOnly.FromDateTime(model.DogumTarihi!.Value), model.Cinsiyet));

        if (!sonuc.Basarili)
        {
            foreach (var hata in sonuc.Hatalar)
                ModelState.AddModelError("", hata);
            model.Sifre = model.SifreTekrar = "";
            return View(model);
        }

        TempData["Mesaj"] = "Kaydınız oluşturuldu. Şimdi giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Giris));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Cikis()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ErisimEngellendi() => View();
}
