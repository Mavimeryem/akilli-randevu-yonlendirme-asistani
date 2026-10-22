using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

[Authorize(Roles = Roller.Doktor)]
public class DoktorController : Controller
{
    private readonly IRandevuService _randevuService;

    public DoktorController(IRandevuService randevuService)
    {
        _randevuService = randevuService;
    }

    public async Task<IActionResult> Takvim(DateOnly? gun)
    {
        if (User.GetDoktorId() is not int doktorId)
            return Forbid();

        var secilenGun = gun ?? DateOnly.FromDateTime(DateTime.Today);
        ViewData["Gun"] = secilenGun;
        return View(await _randevuService.DoktorGunlukAsync(doktorId, secilenGun));
    }
}
