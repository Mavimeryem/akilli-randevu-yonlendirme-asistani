using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AkilliRandevu.Web.Models;

namespace AkilliRandevu.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>Gizlilik ve KVKK aydınlatma metni.</summary>
    public IActionResult Gizlilik()
    {
        return View();
    }

    /// <summary>404, 403 gibi durum kodları için Türkçe sayfa (UseStatusCodePagesWithReExecute).</summary>
    [Route("Home/Durum/{kod:int}")]
    public IActionResult Durum(int kod)
    {
        Response.StatusCode = kod;
        return View(kod);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
