using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
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

    /// <summary>Günün tüm slotları; dolu olanlarda hasta ve işlem butonları.</summary>
    public async Task<IActionResult> Takvim(DateOnly? gun)
    {
        if (User.GetDoktorId() is not int doktorId)
            return Forbid();

        var secilenGun = gun ?? DateOnly.FromDateTime(_randevuService.Simdi);
        var kim = User.ToIslemci();
        var randevular = await _randevuService.DoktorGunlukAsync(doktorId, secilenGun);
        var slotlar = await _randevuService.SlotlariGetirAsync(doktorId, secilenGun);

        var satirlar = slotlar.Select(s =>
        {
            var r = randevular.FirstOrDefault(x => x.BaslangicZamani == s.Baslangic);
            return new TakvimSatiri(s.Baslangic, s.Bitis, s.Durum,
                r is null ? null : new RandevuSatiri(r, _randevuService.IzinVerilenDurumlar(r, kim)));
        }).ToList();

        // Slot düzenine uymayan randevular (ör. ayar değişmeden önce alınmış) da gösterilsin.
        satirlar.AddRange(randevular
            .Where(r => slotlar.All(s => s.Baslangic != r.BaslangicZamani))
            .Select(r => new TakvimSatiri(r.BaslangicZamani, r.BitisZamani, SlotDurumu.Dolu,
                new RandevuSatiri(r, _randevuService.IzinVerilenDurumlar(r, kim)))));

        return View(new DoktorTakvimViewModel
        {
            Gun = secilenGun,
            Satirlar = satirlar.OrderBy(s => s.Baslangic).ToList()
        });
    }
}
