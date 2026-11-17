using System.Security.Claims;
using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Gecmis;
using AkilliRandevu.Entities.Enums;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

/// <summary>
/// Hasta geçmişi. Erişim kuralları iş katmanında (HastaGecmisiService) uygulanır; burası yalnızca sunar.
/// </summary>
[Authorize]
public class GecmisController : Controller
{
    private readonly IHastaGecmisiService _gecmis;
    private readonly IRandevuService _randevu;

    public GecmisController(IHastaGecmisiService gecmis, IRandevuService randevu)
    {
        _gecmis = gecmis;
        _randevu = randevu;
    }

    private int KullaniciId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index(
        int? hastaId, DateOnly? baslangic, DateOnly? bitis, GecmisTuru? tur, RandevuDurumu? durum, int? bolumId, int sayfa = 1)
    {
        var kim = User.ToIslemci();

        // Hasta kimlik vermeden kendi geçmişini açar; doktor/yönetici bir hasta seçmelidir.
        if (hastaId is null)
        {
            if (kim.Rol == Rol.Hasta && kim.HastaId is int kendi)
                hastaId = kendi;
            else if (kim.Rol == Rol.Doktor)
                return RedirectToAction(nameof(Hastalarim));
            else
                return RedirectToAction("Kullanicilar", "Yonetim");
        }

        var filtre = new GecmisFiltresi(baslangic, bitis, tur, durum, bolumId, sayfa);
        if (HastaGecmisiService.FiltreyiDenetle(filtre) is string filtreHatasi)
        {
            TempData["Hata"] = filtreHatasi;
            filtre = new GecmisFiltresi();
        }

        var sonuc = await _gecmis.GetirAsync(hastaId.Value, filtre, kim, KullaniciId);
        if (!sonuc.Basarili)
            return NotFound();

        bool kendisi = kim.Rol == Rol.Hasta;
        var model = new GecmisViewModel
        {
            Gecmis = sonuc.Veri!,
            Filtre = filtre,
            Bolumler = await _randevu.BolumleriDoktorlariylaAsync(),
            KendiGecmisi = kendisi,
            Simdi = _randevu.Simdi
        };
        if (kendisi && (await _gecmis.ErisimKayitlariAsync(hastaId, kim, 20)) is { Basarili: true } erisim)
            model.ErisimKayitlari = erisim.Veri!;

        return View(model);
    }

    [Authorize(Roles = Roller.Doktor)]
    public async Task<IActionResult> Hastalarim()
    {
        if (User.GetDoktorId() is not int doktorId)
            return Forbid();
        return View(await _gecmis.DoktorHastalariAsync(doktorId));
    }

    [Authorize(Roles = Roller.Yonetici)]
    public async Task<IActionResult> ErisimKayitlari()
    {
        var sonuc = await _gecmis.ErisimKayitlariAsync(null, User.ToIslemci(), 200);
        return View(sonuc.Veri ?? []);
    }

    [HttpPost]
    [Authorize(Roles = Roller.Doktor)]
    public async Task<IActionResult> DoktorNotu(int randevuId, string? not, string? returnUrl)
    {
        var sonuc = await _randevu.DoktorNotuKaydetAsync(randevuId, not, User.ToIslemci());
        if (sonuc.Basarili)
            TempData["Mesaj"] = "Muayene notu kaydedildi.";
        else
            TempData["Hata"] = sonuc.Hata;

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction(nameof(Hastalarim));
    }
}
