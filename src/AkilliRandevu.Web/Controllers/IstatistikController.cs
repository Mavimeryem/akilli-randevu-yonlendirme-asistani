using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Istatistik;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkilliRandevu.Web.Controllers;

[Authorize(Roles = Roller.Yonetici)]
public class IstatistikController : Controller
{
    private readonly IIstatistikService _istatistik;
    private readonly IRandevuService _randevu;

    public IstatistikController(IIstatistikService istatistik, IRandevuService randevu)
    {
        _istatistik = istatistik;
        _randevu = randevu;
    }

    private (DateOnly Bas, DateOnly Bit) Aralik(DateOnly? baslangic, DateOnly? bitis)
    {
        var bugun = DateOnly.FromDateTime(_istatistik.Simdi);
        var bit = bitis ?? bugun;
        return (baslangic ?? bit.AddDays(-29), bit);
    }

    public async Task<IActionResult> Index(DateOnly? baslangic, DateOnly? bitis, int? bolumId)
    {
        var (bas, bit) = Aralik(baslangic, bitis);
        var sonuc = await _istatistik.RaporAsync(bas, bit, bolumId);
        if (!sonuc.Basarili)
        {
            TempData["Hata"] = sonuc.Hata;
            return RedirectToAction(nameof(Index));
        }

        return View(new IstatistikViewModel
        {
            Rapor = sonuc.Veri!,
            BolumId = bolumId,
            Bolumler = await _randevu.BolumleriDoktorlariylaAsync(),
            Bugun = DateOnly.FromDateTime(_istatistik.Simdi)
        });
    }

    public async Task<IActionResult> BolumlerCsv(DateOnly? baslangic, DateOnly? bitis, int? bolumId)
    {
        var (bas, bit) = Aralik(baslangic, bitis);
        var csv = await _istatistik.BolumlerCsvAsync(bas, bit, bolumId);
        return csv.Basarili
            ? File(CsvYazici.Bayt(csv.Veri!), "text/csv; charset=utf-8", $"bolum-istatistikleri_{bas:yyyyMMdd}-{bit:yyyyMMdd}.csv")
            : BadRequest(csv.Hata);
    }

    public async Task<IActionResult> RandevularCsv(DateOnly? baslangic, DateOnly? bitis, int? bolumId)
    {
        var (bas, bit) = Aralik(baslangic, bitis);
        var csv = await _istatistik.RandevularCsvAsync(bas, bit, bolumId);
        return csv.Basarili
            ? File(CsvYazici.Bayt(csv.Veri!), "text/csv; charset=utf-8", $"randevular_{bas:yyyyMMdd}-{bit:yyyyMMdd}.csv")
            : BadRequest(csv.Hata);
    }
}
