using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Entities.Enums;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

[Authorize]
public class RandevuController : Controller
{
    private readonly IRandevuService _randevuService;

    public RandevuController(IRandevuService randevuService)
    {
        _randevuService = randevuService;
    }

    /// <summary>Randevu alma: bölüm → doktor → gün seçilir, o günün boş slotları listelenir.</summary>
    [HttpGet]
    [Authorize(Roles = Roller.Hasta)]
    public async Task<IActionResult> Al(int? bolumId, int? doktorId, DateOnly? gun)
    {
        var bugun = DateOnly.FromDateTime(_randevuService.Simdi);
        var model = new RandevuAlViewModel
        {
            Bolumler = await _randevuService.BolumleriDoktorlariylaAsync(),
            BolumId = bolumId,
            DoktorId = doktorId,
            EnErkenGun = bugun,
            EnGecGun = bugun.AddDays(_randevuService.Ayarlar.EnFazlaIleriGun),
        };
        model.Gun = gun is { } g && g >= model.EnErkenGun && g <= model.EnGecGun ? g : bugun;

        if (model.SeciliDoktor is { } doktor)
            model.Slotlar = await _randevuService.SlotlariGetirAsync(doktor.Id, model.Gun);

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = Roller.Hasta)]
    public async Task<IActionResult> Al(int doktorId, DateTime baslangic, int? bolumId)
    {
        // Hasta kimliği formdan değil oturumdan: başkası adına randevu alınamaz.
        if (User.GetHastaId() is not int hastaId)
            return Forbid();

        var sonuc = await _randevuService.OlusturAsync(hastaId, doktorId, baslangic);
        if (!sonuc.Basarili)
        {
            TempData["Hata"] = sonuc.Hata;
            return RedirectToAction(nameof(Al), new { bolumId, doktorId, gun = DateOnly.FromDateTime(baslangic).ToString("yyyy-MM-dd") });
        }

        TempData["Mesaj"] = $"{baslangic:dd.MM.yyyy HH:mm} randevu talebiniz alındı. Doktor onayı bekleniyor.";
        return RedirectToAction("Randevularim", "Hasta");
    }

    /// <summary>
    /// Onay / iptal / tamamlandı / gelmedi. Tek uç noktası var; kimin neyi yapabileceğine
    /// iş katmanı (RandevuDurumKurallari) karar verir.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DurumDegistir(int id, RandevuDurumu durum, string? returnUrl)
    {
        var sonuc = await _randevuService.DurumDegistirAsync(id, durum, User.ToIslemci());
        if (sonuc.Basarili)
            TempData["Mesaj"] = "Randevu güncellendi.";
        else
            TempData["Hata"] = sonuc.Hata;

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
