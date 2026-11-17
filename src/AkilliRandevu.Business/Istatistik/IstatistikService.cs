using System.Globalization;
using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Enums;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.Istatistik;

public interface IIstatistikService
{
    Task<IslemSonucu<IstatistikRaporu>> RaporAsync(DateOnly baslangic, DateOnly bitis, int? bolumId);

    Task<List<DoktorSirasi>> SiraPanosuAsync(int? bolumId);

    /// <summary>Bölüm istatistikleri CSV olarak.</summary>
    Task<IslemSonucu<string>> BolumlerCsvAsync(DateOnly baslangic, DateOnly bitis, int? bolumId);

    /// <summary>Randevu listesi CSV olarak. Hasta adı İÇERMEZ (veri minimizasyonu).</summary>
    Task<IslemSonucu<string>> RandevularCsvAsync(DateOnly baslangic, DateOnly bitis, int? bolumId);

    DateTime Simdi { get; }
}

public class IstatistikService : IIstatistikService
{
    public const int EnFazlaGun = 366;

    private readonly IIstatistikRepository _repo;
    private readonly ISaat _saat;
    private readonly TakvimAyarlari _ayar;

    public IstatistikService(IIstatistikRepository repo, ISaat saat, IOptions<TakvimAyarlari> ayar)
    {
        _repo = repo;
        _saat = saat;
        _ayar = ayar.Value;
    }

    public DateTime Simdi => _saat.Simdi;

    public static string? AraligiDenetle(DateOnly baslangic, DateOnly bitis)
    {
        if (bitis < baslangic)
            return "Bitiş tarihi başlangıç tarihinden önce olamaz.";
        if (bitis.DayNumber - baslangic.DayNumber + 1 > EnFazlaGun)
            return $"Tarih aralığı en fazla {EnFazlaGun} gün olabilir.";
        return null;
    }

    public async Task<IslemSonucu<IstatistikRaporu>> RaporAsync(DateOnly baslangic, DateOnly bitis, int? bolumId)
    {
        if (AraligiDenetle(baslangic, bitis) is string hata)
            return IslemSonucu<IstatistikRaporu>.Hatali(hata);

        var randevular = await _repo.GetRandevularAsync(
            baslangic.ToDateTime(TimeOnly.MinValue), bitis.AddDays(1).ToDateTime(TimeOnly.MinValue), bolumId);
        var doktorlar = await _repo.GetAktifDoktorlarAsync(bolumId);

        return IslemSonucu<IstatistikRaporu>.Tamam(
            IstatistikHesaplayici.Hesapla(randevular, doktorlar, baslangic, bitis, _ayar, _saat.Simdi));
    }

    public async Task<List<DoktorSirasi>> SiraPanosuAsync(int? bolumId)
    {
        var simdi = _saat.Simdi;
        var bugun = DateOnly.FromDateTime(simdi);
        var randevular = await _repo.GetRandevularAsync(
            bugun.ToDateTime(TimeOnly.MinValue), bugun.AddDays(1).ToDateTime(TimeOnly.MinValue), bolumId);
        var doktorlar = await _repo.GetAktifDoktorlarAsync(bolumId);
        return IstatistikHesaplayici.SiraPanosu(randevular, doktorlar, simdi);
    }

    public async Task<IslemSonucu<string>> BolumlerCsvAsync(DateOnly baslangic, DateOnly bitis, int? bolumId)
    {
        var rapor = await RaporAsync(baslangic, bitis, bolumId);
        if (!rapor.Basarili)
            return IslemSonucu<string>.Hatali(rapor.Hata!);

        var satirlar = new List<IReadOnlyList<string?>>
        {
            new[] { "Bölüm", "Toplam randevu", "Aktif randevu", "Kapasite (slot)", "Doluluk %", "İptal %", "Gelmeme %" }
        };
        satirlar.AddRange(rapor.Veri!.Bolumler.Select(b => (IReadOnlyList<string?>)new[]
        {
            b.BolumAdi, b.Toplam.ToString(), b.Aktif.ToString(), b.Kapasite.ToString(),
            Yuzde(b.Doluluk), Yuzde(b.IptalOrani), Yuzde(b.GelmemeOrani)
        }));
        return IslemSonucu<string>.Tamam(CsvYazici.Yaz(satirlar));
    }

    public async Task<IslemSonucu<string>> RandevularCsvAsync(DateOnly baslangic, DateOnly bitis, int? bolumId)
    {
        if (AraligiDenetle(baslangic, bitis) is string hata)
            return IslemSonucu<string>.Hatali(hata);

        var randevular = await _repo.GetRandevularAsync(
            baslangic.ToDateTime(TimeOnly.MinValue), bitis.AddDays(1).ToDateTime(TimeOnly.MinValue), bolumId);

        var satirlar = new List<IReadOnlyList<string?>> { new[] { "Randevu no", "Tarih", "Saat", "Bölüm", "Doktor", "Durum" } };
        satirlar.AddRange(randevular.Select(r => (IReadOnlyList<string?>)new[]
        {
            r.Id.ToString(), r.Baslangic.ToString("dd.MM.yyyy"), r.Baslangic.ToString("HH:mm"),
            r.BolumAdi, r.DoktorAdi, DurumAdi(r.Durum)
        }));
        return IslemSonucu<string>.Tamam(CsvYazici.Yaz(satirlar));
    }

    private static string Yuzde(double? oran) =>
        oran is { } o ? (o * 100).ToString("0.0", CultureInfo.GetCultureInfo("tr-TR")) : "";

    public static string DurumAdi(RandevuDurumu d) => d switch
    {
        RandevuDurumu.Beklemede => "Beklemede",
        RandevuDurumu.Onaylandi => "Onaylandı",
        RandevuDurumu.Tamamlandi => "Tamamlandı",
        RandevuDurumu.IptalEdildi => "İptal edildi",
        RandevuDurumu.Gelmedi => "Gelmedi",
        _ => d.ToString()
    };
}
