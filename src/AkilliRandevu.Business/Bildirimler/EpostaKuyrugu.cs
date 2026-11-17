using AkilliRandevu.Business.Abstract;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Enums;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.Bildirimler;

public record KuyrukTuruSonucu(int Gonderilen, int Bekleyen, int Basarisiz, int Atlanan);

public interface IEpostaKuyrugu
{
    /// <summary>Giden kutusundaki bekleyen e-postaları gönderir ve sonuçları kaydeder.</summary>
    Task<KuyrukTuruSonucu> BekleyenleriGonderAsync(CancellationToken ct = default);
}

/// <summary>
/// Giden kutusu (outbox) işleyicisi. Her turda en fazla TurBasinaEnFazlaEposta kayıt işlenir (hız sınırı).
///   Gönderildi      → Durum = Gonderildi, mesaj kimliği saklanır
///   Kalıcı hata     → Durum = Basarisiz (tekrar denenmez)
///   Geçici hata     → Bekliyor kalır, sonraki turda tekrar denenir; toplam deneme sınırı aşılınca Basarisiz
///   Yapılandırılmadı→ Durum = Atlandi
/// </summary>
public class EpostaKuyrugu : IEpostaKuyrugu
{
    private readonly IBildirimRepository _bildirimler;
    private readonly IEpostaGonderici _gonderici;
    private readonly ISaat _saat;
    private readonly EpostaAyarlari _eposta;
    private readonly BildirimAyarlari _ayar;

    public EpostaKuyrugu(
        IBildirimRepository bildirimler,
        IEpostaGonderici gonderici,
        ISaat saat,
        IOptions<EpostaAyarlari> eposta,
        IOptions<BildirimAyarlari> ayar)
    {
        _bildirimler = bildirimler;
        _gonderici = gonderici;
        _saat = saat;
        _eposta = eposta.Value;
        _ayar = ayar.Value;
    }

    public async Task<KuyrukTuruSonucu> BekleyenleriGonderAsync(CancellationToken ct = default)
    {
        int gonderilen = 0, bekleyen = 0, basarisiz = 0, atlanan = 0;
        var liste = await _bildirimler.GetBekleyenEpostalarAsync(_ayar.TurBasinaEnFazlaEposta);

        foreach (var e in liste)
        {
            ct.ThrowIfCancellationRequested();

            var alici = string.IsNullOrWhiteSpace(_eposta.YonlendirmeAdresi) ? e.Alici : _eposta.YonlendirmeAdresi!;
            var metin = e.Bildirim.Mesaj;
            if (alici != e.Alici)
                metin += $"\n\n[Demo yönlendirmesi – asıl alıcı: {e.Alici}]";
            metin += "\n\n— Bu e-posta Akıllı Randevu ve Poliklinik Yönlendirme Asistanı tarafından otomatik gönderilmiştir.";

            var sonuc = await _gonderici.GonderAsync(new EpostaMesaji(alici, e.Bildirim.Baslik, metin), ct);
            e.DenemeSayisi += Math.Max(1, sonuc.DenemeSayisi);

            switch (sonuc.Tur)
            {
                case EpostaSonucTuru.Gonderildi:
                    e.Durum = GonderimDurumu.Gonderildi;
                    e.GonderimTarihi = _saat.UtcSimdi;
                    e.DisMesajId = sonuc.MesajId;
                    e.SonHata = null;
                    gonderilen++;
                    break;
                case EpostaSonucTuru.KaliciHata:
                    e.Durum = GonderimDurumu.Basarisiz;
                    e.SonHata = Kisalt(sonuc.Hata);
                    basarisiz++;
                    break;
                case EpostaSonucTuru.Yapilandirilmadi:
                    e.Durum = GonderimDurumu.Atlandi;
                    e.SonHata = Kisalt(sonuc.Hata);
                    atlanan++;
                    break;
                default: // GeciciHata
                    e.SonHata = Kisalt(sonuc.Hata);
                    if (e.DenemeSayisi >= _ayar.EnFazlaKuyrukDenemesi)
                    {
                        e.Durum = GonderimDurumu.Basarisiz;
                        basarisiz++;
                    }
                    else
                    {
                        bekleyen++;
                    }
                    break;
            }

            await _bildirimler.UpdateEpostaAsync(e);
        }

        return new KuyrukTuruSonucu(gonderilen, bekleyen, basarisiz, atlanan);
    }

    private static string? Kisalt(string? s) => s is { Length: > 500 } ? s[..500] : s;
}
