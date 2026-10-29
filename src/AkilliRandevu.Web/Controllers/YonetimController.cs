using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

[Authorize(Roles = Roller.Yonetici)]
public class YonetimController : Controller
{
    private readonly IKullaniciService _kullaniciService;
    private readonly IRandevuService _randevuService;

    public YonetimController(IKullaniciService kullaniciService, IRandevuService randevuService)
    {
        _kullaniciService = kullaniciService;
        _randevuService = randevuService;
    }

    public async Task<IActionResult> Kullanicilar() =>
        View(await _kullaniciService.TumKullanicilarAsync());

    public async Task<IActionResult> Randevular(DateOnly? gun, int? bolumId)
    {
        var secilenGun = gun ?? DateOnly.FromDateTime(_randevuService.Simdi);
        var kim = User.ToIslemci();
        return View(new YonetimRandevularViewModel
        {
            Gun = secilenGun,
            BolumId = bolumId,
            Bolumler = await _randevuService.BolumleriDoktorlariylaAsync(),
            Randevular = (await _randevuService.GunlukTumuAsync(secilenGun, bolumId))
                .Select(r => new RandevuSatiri(r, _randevuService.IzinVerilenDurumlar(r, kim)))
                .ToList()
        });
    }
}
