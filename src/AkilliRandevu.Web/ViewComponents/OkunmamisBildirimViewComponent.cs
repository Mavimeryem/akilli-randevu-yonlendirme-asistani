using System.Security.Claims;
using AkilliRandevu.Business.Bildirimler;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.ViewComponents;

/// <summary>Menüdeki "Bildirimler (3)" rozeti.</summary>
public class OkunmamisBildirimViewComponent : ViewComponent
{
    private readonly IBildirimService _bildirim;

    public OkunmamisBildirimViewComponent(IBildirimService bildirim)
    {
        _bildirim = bildirim;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int sayi = 0;
        if (UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) is string id && int.TryParse(id, out var kullaniciId))
            sayi = await _bildirim.OkunmamisSayisiAsync(kullaniciId);
        return View(sayi);
    }
}
