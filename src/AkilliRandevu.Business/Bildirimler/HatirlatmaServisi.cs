using AkilliRandevu.Business.Abstract;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Enums;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.Bildirimler;

public interface IHatirlatmaServisi
{
    /// <summary>Yaklaşan randevular için hatırlatma bildirimi oluşturur. Oluşturulan hatırlatma sayısını döner.</summary>
    Task<int> HatirlatmalariOlusturAsync(CancellationToken ct = default);
}

/// <summary>
/// Zamanlama kuralı: randevuya HatirlatmaSaatOnce (varsayılan 24) saat veya daha az kalmışsa ve randevu
/// beklemede/onaylıysa bir kez hatırlatılır. Randevu.HatirlatmaTarihi dolu olanlar atlanır; böylece
/// servis kaç kez çalışırsa çalışsın aynı randevuya ikinci hatırlatma gitmez.
/// </summary>
public class HatirlatmaServisi : IHatirlatmaServisi
{
    private readonly IRandevuRepository _randevular;
    private readonly IBildirimService _bildirim;
    private readonly IIslemYoneticisi _islem;
    private readonly ISaat _saat;
    private readonly BildirimAyarlari _ayar;

    public HatirlatmaServisi(
        IRandevuRepository randevular,
        IBildirimService bildirim,
        IIslemYoneticisi islem,
        ISaat saat,
        IOptions<BildirimAyarlari> ayar)
    {
        _randevular = randevular;
        _bildirim = bildirim;
        _islem = islem;
        _saat = saat;
        _ayar = ayar.Value;
    }

    public async Task<int> HatirlatmalariOlusturAsync(CancellationToken ct = default)
    {
        var simdi = _saat.Simdi;
        var sinir = simdi.AddHours(_ayar.HatirlatmaSaatOnce);
        var randevular = await _randevular.GetHatirlatilacaklarAsync(simdi, sinir);

        int sayi = 0;
        foreach (var r in randevular)
        {
            ct.ThrowIfCancellationRequested();

            var doktor = $"{r.Doktor.Unvan} {r.Doktor.Kullanici.Ad} {r.Doktor.Kullanici.Soyad}";
            var not = r.Durum == RandevuDurumu.Beklemede ? " Randevunuz henüz doktor onayı bekliyor." : "";
            var mesaj = $"{r.BaslangicZamani:dd.MM.yyyy HH:mm} tarihinde {r.Doktor.Bolum.Ad} bölümünde " +
                        $"{doktor} ile randevunuz var.{not} Gelemeyecekseniz lütfen randevunuzu iptal edin.";

            // Bildirim + hatırlatma işareti birlikte: ya ikisi ya hiçbiri (aynı randevu iki kez hatırlatılmaz).
            await _islem.TransactionIcindeAsync(async () =>
            {
                await _bildirim.OlusturAsync(r.Hasta.KullaniciId, "Randevu hatırlatma", mesaj);
                r.HatirlatmaTarihi = _saat.UtcSimdi;
                await _randevular.UpdateAsync(r);
                return true;
            });
            sayi++;
        }
        return sayi;
    }
}
