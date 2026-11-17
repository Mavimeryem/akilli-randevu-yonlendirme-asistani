using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.Bildirimler;

/// <summary>
/// Uygulamayla birlikte arka planda çalışır; her TurAraligiSaniye'de bir:
///   1) yaklaşan randevular için hatırlatma oluşturur,
///   2) giden kutusundaki e-postaları gönderir.
/// Bir turdaki hata (ör. e-posta servisi çökmüş) uygulamayı durdurmaz; loglanır ve sonraki turda tekrar denenir.
/// </summary>
public sealed class BildirimArkaPlanServisi : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BildirimAyarlari _ayar;
    private readonly ILogger<BildirimArkaPlanServisi> _log;

    public BildirimArkaPlanServisi(
        IServiceScopeFactory scopeFactory,
        IOptions<BildirimAyarlari> ayar,
        ILogger<BildirimArkaPlanServisi> log)
    {
        _scopeFactory = scopeFactory;
        _ayar = ayar.Value;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken durdur)
    {
        if (!_ayar.ArkaPlanEtkin)
        {
            _log.LogInformation("Bildirim arka plan servisi kapalı (Bildirim:ArkaPlanEtkin = false).");
            return;
        }

        using var zamanlayici = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(5, _ayar.TurAraligiSaniye)));
        do
        {
            await TurCalistirAsync(durdur);
        }
        while (await zamanlayici.WaitForNextTickAsync(durdur));
    }

    private async Task TurCalistirAsync(CancellationToken durdur)
    {
        try
        {
            // DbContext scoped olduğu için her tur kendi kapsamını (scope) açar.
            using var scope = _scopeFactory.CreateScope();
            var hatirlatma = scope.ServiceProvider.GetRequiredService<IHatirlatmaServisi>();
            var kuyruk = scope.ServiceProvider.GetRequiredService<IEpostaKuyrugu>();

            int hatirlatilan = await hatirlatma.HatirlatmalariOlusturAsync(durdur);
            var sonuc = await kuyruk.BekleyenleriGonderAsync(durdur);

            if (hatirlatilan > 0 || sonuc.Gonderilen + sonuc.Bekleyen + sonuc.Basarisiz + sonuc.Atlanan > 0)
                _log.LogInformation(
                    "Bildirim turu: {Hatirlatma} hatırlatma, {Gonderilen} gönderildi, {Bekleyen} tekrar denenecek, {Basarisiz} başarısız, {Atlanan} atlandı",
                    hatirlatilan, sonuc.Gonderilen, sonuc.Bekleyen, sonuc.Basarisiz, sonuc.Atlanan);
        }
        catch (OperationCanceledException) when (durdur.IsCancellationRequested)
        {
            // Uygulama kapanıyor.
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Bildirim turu hata verdi; sonraki turda tekrar denenecek.");
        }
    }
}
