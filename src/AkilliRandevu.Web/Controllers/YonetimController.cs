using System.Security.Claims;
using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Bildirimler;
using AkilliRandevu.Web.Infrastructure;
using AkilliRandevu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Web.Controllers;

[Authorize(Roles = Roller.Yonetici)]
public class YonetimController : Controller
{
    private readonly IKullaniciService _kullaniciService;
    private readonly IRandevuService _randevuService;
    private readonly IBildirimService _bildirimService;

    public YonetimController(IKullaniciService kullaniciService, IRandevuService randevuService, IBildirimService bildirimService)
    {
        _kullaniciService = kullaniciService;
        _randevuService = randevuService;
        _bildirimService = bildirimService;
    }

    public async Task<IActionResult> Kullanicilar() =>
        View(await _kullaniciService.TumKullanicilarAsync());

    public async Task<IActionResult> Randevular(DateOnly? gun, int? bolumId)
    {
        var secilenGun = gun ?? DateOnly.FromDateTime(_randevuService.Simdi);
        var kim = User.ToIslemci();
        return View(new YonetimRandevularViewModel
        {
            Gun = secilenGun,
            BolumId = bolumId,
            Bolumler = await _randevuService.BolumleriDoktorlariylaAsync(),
            Randevular = (await _randevuService.GunlukTumuAsync(secilenGun, bolumId))
                .Select(r => new RandevuSatiri(r, _randevuService.IzinVerilenDurumlar(r, kim)))
                .ToList()
        });
    }

    /// <summary>E-posta entegrasyonunun durumu: giden kutusundaki son kayıtlar.</summary>
    public async Task<IActionResult> EpostaGonderimleri([FromServices] IOptions<EpostaAyarlari> ayar)
    {
        ViewData["Yapilandirildi"] = ayar.Value.Yapilandirildi;
        ViewData["Yonlendirme"] = ayar.Value.YonlendirmeAdresi;
        return View(await _bildirimService.SonEpostaGonderimleriAsync());
    }

    /// <summary>
    /// API entegrasyonunu doğrudan dener: e-postayı kuyruğa koymadan hemen gönderir ve sonucu gösterir.
    /// Alıcı: yönlendirme adresi ayarlıysa o, değilse giriş yapan yöneticinin e-postası.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TestEpostasi(
        [FromServices] IEpostaGonderici gonderici,
        [FromServices] IOptions<EpostaAyarlari> ayar,
        CancellationToken ct)
    {
        var alici = string.IsNullOrWhiteSpace(ayar.Value.YonlendirmeAdresi)
            ? User.FindFirstValue(ClaimTypes.Email)!
            : ayar.Value.YonlendirmeAdresi!;

        var sonuc = await gonderici.GonderAsync(new EpostaMesaji(alici,
            "Akıllı Randevu – test e-postası",
            $"Bu bir test e-postasıdır. Gönderim zamanı (UTC): {DateTime.UtcNow:dd.MM.yyyy HH:mm:ss}"), ct);

        if (sonuc.Basarili)
            TempData["Mesaj"] = $"Test e-postası {alici} adresine gönderildi ({sonuc.DenemeSayisi}. denemede). Mesaj kimliği: {sonuc.MesajId}";
        else
            TempData["Hata"] = $"Test e-postası gönderilemedi ({sonuc.Tur}, {sonuc.DenemeSayisi} deneme): {sonuc.Hata}";

        return RedirectToAction(nameof(EpostaGonderimleri));
    }
}
