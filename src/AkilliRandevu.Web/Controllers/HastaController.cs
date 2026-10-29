using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

/// <summary>Yalnızca Hasta rolü. Giriş yapmamış -> giriş sayfası, başka rol -> erişim engellendi.</summary>
[Authorize(Roles = Roller.Hasta)]
public class HastaController : Controller
{
    private readonly IRandevuService _randevuService;

    public HastaController(IRandevuService randevuService)
    {
        _randevuService = randevuService;
    }

    public async Task<IActionResult> Randevularim()
    {
        // Hasta kimliği URL'den değil oturumdan alınır: başkasının randevuları görüntülenemez.
        if (User.GetHastaId() is not int hastaId)
            return Forbid();

        var kim = User.ToIslemci();
        var satirlar = (await _randevuService.HastaGecmisiAsync(hastaId))
            .Select(r => new RandevuSatiri(r, _randevuService.IzinVerilenDurumlar(r, kim)))
            .ToList();
        return View(satirlar);
    }
}
