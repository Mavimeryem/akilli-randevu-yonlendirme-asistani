using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

[Authorize(Roles = Roller.Yonetici)]
public class YonetimController : Controller
{
    private readonly IKullaniciService _kullaniciService;

    public YonetimController(IKullaniciService kullaniciService)
    {
        _kullaniciService = kullaniciService;
    }

    public async Task<IActionResult> Kullanicilar() =>
        View(await _kullaniciService.TumKullanicilarAsync());
}
