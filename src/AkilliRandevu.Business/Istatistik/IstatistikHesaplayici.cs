using AkilliRandevu.Business.Takvim;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Istatistik;

/// <summary>
/// Randevu istatistiklerinin hesap mantığı. Saf fonksiyondur: veritabanına gitmez, aynı girdiye aynı sonucu verir.
///
/// Metrik tanımları:
///   Toplam          = aralıkta başlayan tüm randevular (iptaller dahil)
///   Aktif           = iptal edilmemiş randevular (planlanan + gerçekleşen)
///   Kapasite        = Σ (aktif doktor × aralıktaki çalışma günü × o günün slot sayısı)
///   Doluluk         = Aktif / Kapasite                     – kapasite 0 ise hesaplanmaz (null)
///   İptal oranı     = İptal / Toplam                       – toplam 0 ise null
///   Gelmeme oranı   = Gelmedi / (Tamamlandı + Gelmedi)     – yalnızca SONUCU GİRİLMİŞ randevular
///   Sonuçlanmamış   = saati geçmiş ama hâlâ beklemede/onaylı randevular (eksik veri; gelmeme oranına katılmaz)
///   Yoğunluk        = gün × saat başına aktif randevu sayısı (çalışma günleri ve mesai saatleri)
/// </summary>
public static class IstatistikHesaplayici
{
    public static IstatistikRaporu Hesapla(
        IReadOnlyList<RandevuOzeti> randevular,
        IReadOnlyList<DoktorOzeti> doktorlar,
        DateOnly baslangic,
        DateOnly bitis,
        TakvimAyarlari ayar,
        DateTime simdi)
    {
        // --- Kapasite: her gün için slot sayısı (hafta sonu kapalıysa 0) ---
        var gunler = new List<DateOnly>();
        for (var g = baslangic; g <= bitis; g = g.AddDays(1))
            gunler.Add(g);
        var gunlukSlot = gunler.ToDictionary(g => g, g => SlotUretici.Araliklar(g, ayar).Count);
        int doktorBasinaKapasite = gunlukSlot.Values.Sum();

        var genel = Ozetle(randevular, doktorlar.Count * doktorBasinaKapasite, simdi);

        // --- Bölüm kırılımı (randevusu olmayan bölümler de kapasiteleriyle listelenir) ---
        var bolumAdlari = doktorlar.Select(d => (d.BolumId, d.BolumAdi))
            .Concat(randevular.Select(r => (r.BolumId, r.BolumAdi)))
            .DistinctBy(x => x.BolumId)
            .ToList();
        var bolumler = bolumAdlari
            .Select(b =>
            {
                var o = Ozetle(randevular.Where(r => r.BolumId == b.BolumId).ToList(),
                               doktorlar.Count(d => d.BolumId == b.BolumId) * doktorBasinaKapasite, simdi);
                return new BolumIstatistigi(b.BolumId, b.BolumAdi, o.Toplam, o.Aktif, o.Kapasite, o.Doluluk, o.IptalOrani, o.GelmemeOrani);
            })
            .OrderByDescending(b => b.Doluluk ?? -1).ThenBy(b => b.BolumAdi)
            .ToList();

        // --- Gün × saat yoğunluğu (yalnızca aktif randevular, çalışma günleri ve mesai saatleri) ---
        var calismaGunleri = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
            .Concat(ayar.HaftaSonuAcik ? [DayOfWeek.Saturday, DayOfWeek.Sunday] : Array.Empty<DayOfWeek>())
            .ToList();
        var saatler = Enumerable.Range(ayar.MesaiBaslangic.Hour, Math.Max(0, ayar.MesaiBitis.Hour - ayar.MesaiBaslangic.Hour)).ToList();
        var aktifler = randevular.Where(r => r.Durum != RandevuDurumu.IptalEdildi).ToList();
        var yogunluk = (from gun in calismaGunleri
                        from saat in saatler
                        select new YogunlukHucresi(gun, saat,
                            aktifler.Count(r => r.Baslangic.DayOfWeek == gun && r.Baslangic.Hour == saat)))
                       .ToList();

        // --- Günlük trend ---
        var gunluk = gunler
            .Select(g => new GunlukSayi(g,
                randevular.Count(r => DateOnly.FromDateTime(r.Baslangic) == g && r.Durum != RandevuDurumu.IptalEdildi),
                randevular.Count(r => DateOnly.FromDateTime(r.Baslangic) == g && r.Durum == RandevuDurumu.IptalEdildi)))
            .ToList();

        return new IstatistikRaporu(
            baslangic, bitis,
            genel.Toplam, genel.Aktif, genel.Iptal, genel.Tamamlanan, genel.Gelmedi, genel.Sonuclanmamis,
            genel.Kapasite, genel.Doluluk, genel.IptalOrani, genel.GelmemeOrani,
            bolumler, yogunluk, calismaGunleri, saatler, gunluk);
    }

    private record Ozet(int Toplam, int Aktif, int Iptal, int Tamamlanan, int Gelmedi, int Sonuclanmamis,
                        int Kapasite, double? Doluluk, double? IptalOrani, double? GelmemeOrani);

    private static Ozet Ozetle(IReadOnlyCollection<RandevuOzeti> r, int kapasite, DateTime simdi)
    {
        int toplam = r.Count;
        int iptal = r.Count(x => x.Durum == RandevuDurumu.IptalEdildi);
        int aktif = toplam - iptal;
        int tamamlanan = r.Count(x => x.Durum == RandevuDurumu.Tamamlandi);
        int gelmedi = r.Count(x => x.Durum == RandevuDurumu.Gelmedi);
        int sonuclanmamis = r.Count(x => x.Baslangic <= simdi
                                      && (x.Durum == RandevuDurumu.Beklemede || x.Durum == RandevuDurumu.Onaylandi));

        return new Ozet(toplam, aktif, iptal, tamamlanan, gelmedi, sonuclanmamis, kapasite,
            Oran(aktif, kapasite), Oran(iptal, toplam), Oran(gelmedi, tamamlanan + gelmedi));
    }

    /// <summary>Payda 0 ise oran tanımsızdır (null) – "%0" demek yanıltıcı olurdu.</summary>
    public static double? Oran(int pay, int payda) => payda == 0 ? null : (double)pay / payda;

    /// <summary>Bekleme salonu ekranı için: "Deniz Acar" → "D*** A***".</summary>
    public static string AdMaskele(string ad, string soyad) =>
        $"{Ilk(ad)}*** {Ilk(soyad)}***";

    private static string Ilk(string s) => string.IsNullOrWhiteSpace(s) ? "?" : s.Trim()[..1].ToUpperInvariant();

    /// <summary>
    /// Sıra panosu: bugünkü randevulardan her doktorun şu anki ve sıradaki hastasını, kalan randevu sayısını
    /// ve tahmini beklemeyi hesaplar. "Şu an" = başlamış ama bitmemiş, sonucu girilmemiş randevu.
    /// </summary>
    public static List<DoktorSirasi> SiraPanosu(
        IReadOnlyList<RandevuOzeti> bugun, IReadOnlyList<DoktorOzeti> doktorlar, DateTime simdi)
    {
        static bool Bekleyen(RandevuOzeti r) => r.Durum is RandevuDurumu.Beklemede or RandevuDurumu.Onaylandi;
        static SiradakiHasta Hasta(RandevuOzeti r) => new(r.Id, AdMaskele(r.HastaAdi, r.HastaSoyadi), r.Baslangic, r.Bitis);

        return doktorlar.Select(d =>
            {
                var liste = bugun.Where(r => r.DoktorId == d.DoktorId && r.Durum != RandevuDurumu.IptalEdildi)
                                 .OrderBy(r => r.Baslangic).ToList();
                var suAnki = liste.FirstOrDefault(r => Bekleyen(r) && r.Baslangic <= simdi && simdi < r.Bitis);
                var kalanlar = liste.Where(r => Bekleyen(r) && r.Baslangic > simdi).ToList();
                var siradaki = kalanlar.FirstOrDefault();

                return new DoktorSirasi(
                    d.DoktorId, d.DoktorAdi, d.BolumAdi,
                    suAnki is null ? null : Hasta(suAnki),
                    siradaki is null ? null : Hasta(siradaki),
                    kalanlar.Count,
                    siradaki is null ? null : (int)Math.Ceiling((siradaki.Baslangic - simdi).TotalMinutes),
                    liste.Count,
                    liste.Count(r => r.Durum == RandevuDurumu.Tamamlandi),
                    liste.Count(r => r.Durum == RandevuDurumu.Gelmedi));
            })
            .OrderBy(s => s.BolumAdi).ThenBy(s => s.DoktorAdi)
            .ToList();
    }
}
