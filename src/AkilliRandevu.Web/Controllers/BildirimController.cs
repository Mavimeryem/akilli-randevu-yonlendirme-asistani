using System.Security.Claims;
using AkilliRandevu.Business.Bildirimler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

/// <summary>Uygulama içi bildirimler. Her kullanıcı yalnızca kendi bildirimlerini görür (kimlik oturumdan).</summary>
[Authorize]
public class BildirimController : Controller
{
    private readonly IBildirimService _bildirim;

    public BildirimController(IBildirimService bildirim)
    {
        _bildirim = bildirim;
    }

    private int KullaniciId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index() =>
        View(await _bildirim.KullaniciBildirimleriAsync(KullaniciId));

    [HttpPost]
    public async Task<IActionResult> OkunduYap(int? id)
    {
        await _bildirim.OkunduYapAsync(KullaniciId, id);
        return RedirectToAction(nameof(Index));
    }
}
