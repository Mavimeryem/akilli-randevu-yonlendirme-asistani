namespace AkilliRandevu.Business.Istatistik;

public record BolumIstatistigi(
    int BolumId,
    string BolumAdi,
    int Toplam,
    int Aktif,
    int Kapasite,
    double? Doluluk,
    double? IptalOrani,
    double? GelmemeOrani);

/// <summary>Gün × saat yoğunluk hücresi (aktif randevu sayısı).</summary>
public record YogunlukHucresi(DayOfWeek Gun, int Saat, int Sayi);

public record GunlukSayi(DateOnly Gun, int Aktif, int Iptal);

public record IstatistikRaporu(
    DateOnly Baslangic,
    DateOnly Bitis,
    int Toplam,
    int Aktif,
    int Iptal,
    int Tamamlanan,
    int Gelmedi,
    /// <summary>Saati geçmiş ama sonucu (tamamlandı/gelmedi) girilmemiş randevular – eksik veri.</summary>
    int Sonuclanmamis,
    int Kapasite,
    double? Doluluk,
    double? IptalOrani,
    double? GelmemeOrani,
    IReadOnlyList<BolumIstatistigi> Bolumler,
    IReadOnlyList<YogunlukHucresi> Yogunluk,
    IReadOnlyList<DayOfWeek> Gunler,
    IReadOnlyList<int> Saatler,
    IReadOnlyList<GunlukSayi> Gunluk);

/// <summary>Sıra panosunda bir hasta: adı maskelenir (bekleme salonu ekranında gösterilebilir).</summary>
public record SiradakiHasta(int RandevuId, string MaskeliAd, DateTime Baslangic, DateTime Bitis);

public record DoktorSirasi(
    int DoktorId,
    string DoktorAdi,
    string BolumAdi,
    SiradakiHasta? SuAnki,
    SiradakiHasta? Siradaki,
    int KalanRandevu,
    int? TahminiBeklemeDakika,
    int BugunToplam,
    int Tamamlanan,
    int Gelmedi);
