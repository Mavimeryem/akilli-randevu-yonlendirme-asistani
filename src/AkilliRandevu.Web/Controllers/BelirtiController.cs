using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Yonlendirme;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

[Authorize(Roles = Roller.Hasta)]
public class BelirtiController : Controller
{
    private readonly IBelirtiService _belirtiService;

    public BelirtiController(IBelirtiService belirtiService)
    {
        _belirtiService = belirtiService;
    }

    [HttpGet]
    public async Task<IActionResult> Giris()
    {
        var katalog = await _belirtiService.BelirtileriGetirAsync();
        return View(new BelirtiGirisViewModel
        {
            Belirtiler = katalog.Select(b => new BelirtiSecimi { BelirtiId = b.Id, Ad = b.Ad }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Giris(BelirtiGirisViewModel model)
    {
        if (User.GetHastaId() is not int hastaId)
            return Forbid();

        if (ModelState.IsValid)
        {
            var secilenler = model.Belirtiler
                .Where(b => b.Secili)
                .Select(b => new SecilenBelirti(b.BelirtiId, b.Siddet))
                .ToList();

            var sonuc = await _belirtiService.DegerlendirAsync(hastaId, secilenler, model.Aciklama);
            if (sonuc.Basarili)
                return RedirectToAction(nameof(Sonuc), new { id = sonuc.Veri });

            ModelState.AddModelError("", sonuc.Hata!);
        }

        // Hata: belirti adları formdan gelmediği için katalogdan yeniden doldurulur, seçimler korunur.
        var katalog = await _belirtiService.BelirtileriGetirAsync();
        var gelen = model.Belirtiler.GroupBy(b => b.BelirtiId).ToDictionary(g => g.Key, g => g.First());
        model.Belirtiler = katalog.Select(b =>
        {
            gelen.TryGetValue(b.Id, out var g);
            return new BelirtiSecimi
            {
                BelirtiId = b.Id,
                Ad = b.Ad,
                Secili = g?.Secili ?? false,
                Siddet = Math.Clamp(g?.Siddet ?? 5, 1, 10)
            };
        }).ToList();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Sonuc(int id)
    {
        var sonuc = await _belirtiService.KayitGetirAsync(id, User.ToIslemci());
        if (!sonuc.Basarili)
            return NotFound();
        return View(sonuc.Veri);
    }

    [HttpGet]
    public async Task<IActionResult> Kayitlarim()
    {
        if (User.GetHastaId() is not int hastaId)
            return Forbid();
        return View(await _belirtiService.HastaKayitlariAsync(hastaId));
    }
}
