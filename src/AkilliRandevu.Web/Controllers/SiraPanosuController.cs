using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Istatistik;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

/// <summary>Bugünkü sıra durumu. Personel (doktor, yönetici) görür; hasta adları maskelidir.</summary>
[Authorize(Roles = Roller.Personel)]
public class SiraPanosuController : Controller
{
    private readonly IIstatistikService _istatistik;
    private readonly IRandevuService _randevu;

    public SiraPanosuController(IIstatistikService istatistik, IRandevuService randevu)
    {
        _istatistik = istatistik;
        _randevu = randevu;
    }

    public async Task<IActionResult> Index(int? bolumId) => View(new SiraPanosuViewModel
    {
        Siralar = await _istatistik.SiraPanosuAsync(bolumId),
        BolumId = bolumId,
        Bolumler = await _randevu.BolumleriDoktorlariylaAsync(),
        Simdi = _istatistik.Simdi
    });
}
