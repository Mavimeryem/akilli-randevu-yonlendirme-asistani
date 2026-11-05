using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using K = AkilliRandevu.Business.Yonlendirme.BelirtiKodlari;

namespace AkilliRandevu.Business.Yonlendirme;

/// <summary>
/// Kural tabanlı ön yönlendirme. Saf fonksiyondur: veritabanına gitmez, aynı girdiye her zaman aynı sonucu verir.
///
/// 1) Bölüm puanı  = Σ (BelirtiBolum.AgirlikPuani × hastanın girdiği şiddet)
///    En yüksek puanlı bölüm önerilir; ilk 3 bölüm yüzdeleriyle gösterilir.
/// 2) Aciliyet     = belirtilerin en yüksek şiddetine göre başlangıç seviyesi,
///    acil durum kurallarından biri tutarsa o kuralın seviyesine yükseltilir.
/// 3) Belirsizlik  = eşleşme yoksa ya da en iyi bölüm baskın değilse "belirsiz" denir ve Dahiliye önerilir.
///
/// Bu bir tanı değildir; yalnızca hangi polikliniğe başvurulacağı konusunda ön yönlendirmedir.
/// </summary>
public static class YonlendirmeMotoru
{
    public const int EnFazlaOneri = 3;

    /// <summary>En uygun bölümün payı bunun altındaysa sonuç belirsiz sayılır.</summary>
    public const int BaskinlikEsigiYuzde = 40;

    /// <summary>İlk iki bölüm arasındaki fark bundan azsa sonuç belirsiz sayılır.</summary>
    public const int YakinlikEsigiYuzde = 10;

    private record AcilKurali(AciliyetSeviyesi Seviye, string Uyari, Func<IReadOnlyDictionary<int, int>, bool> Kosul);

    // Kırmızı bayrak kuralları. Sözlük: belirtiId -> şiddet.
    private static readonly AcilKurali[] Kurallar =
    [
        new(AciliyetSeviyesi.Acil, "Şiddetli göğüs ağrısı kalp kaynaklı olabilir.",
            s => Siddet(s, K.GogusAgrisi) >= 7),
        new(AciliyetSeviyesi.Acil, "Göğüs ağrısıyla birlikte nefes darlığı acil değerlendirme gerektirir.",
            s => Var(s, K.GogusAgrisi) && Var(s, K.NefesDarligi)),
        new(AciliyetSeviyesi.Acil, "Şiddetli nefes darlığı acil değerlendirme gerektirir.",
            s => Siddet(s, K.NefesDarligi) >= 8),
        new(AciliyetSeviyesi.Acil, "Uyuşmayla birlikte görme bozukluğu veya baş dönmesi inme belirtisi olabilir.",
            s => Var(s, K.Uyusma) && (Var(s, K.BulanikGorme) || Var(s, K.BasDonmesi))),
        new(AciliyetSeviyesi.Yuksek, "Yüksek ateşle birlikte şiddetli baş ağrısı kısa sürede değerlendirilmelidir.",
            s => Siddet(s, K.Ates) >= 7 && Siddet(s, K.BasAgrisi) >= 7),
        new(AciliyetSeviyesi.Yuksek, "Şiddetli karın ağrısı kısa sürede değerlendirilmelidir.",
            s => Siddet(s, K.KarinAgrisi) >= 8),
        new(AciliyetSeviyesi.Yuksek, "Çarpıntıyla birlikte baş dönmesi kısa sürede değerlendirilmelidir.",
            s => Var(s, K.Carpinti) && Var(s, K.BasDonmesi)),
    ];

    private static int Siddet(IReadOnlyDictionary<int, int> s, int belirtiId) => s.GetValueOrDefault(belirtiId);

    private static bool Var(IReadOnlyDictionary<int, int> s, int belirtiId) => s.ContainsKey(belirtiId);

    /// <param name="secilenler">Hastanın belirtileri ve şiddetleri.</param>
    /// <param name="eslesmeler">Seçilen belirtilerin aktif bölümlerle eşleşmeleri (Bolum dolu olmalı).</param>
    /// <param name="varsayilanBolum">Eşleşme yoksa veya sonuç belirsizse önerilecek genel bölüm (Dahiliye).</param>
    public static YonlendirmeSonucu Degerlendir(
        IReadOnlyCollection<SecilenBelirti> secilenler,
        IReadOnlyCollection<BelirtiBolum> eslesmeler,
        Bolum? varsayilanBolum)
    {
        var siddetler = secilenler.ToDictionary(x => x.BelirtiId, x => x.Siddet);

        // --- 1) Bölüm puanları ---
        var puanlar = eslesmeler
            .Where(e => siddetler.ContainsKey(e.BelirtiId))
            .GroupBy(e => new { e.BolumId, e.Bolum.Ad })
            .Select(g => new { g.Key.BolumId, g.Key.Ad, Puan = g.Sum(e => e.AgirlikPuani * siddetler[e.BelirtiId]) })
            .OrderByDescending(x => x.Puan).ThenBy(x => x.Ad)
            .ToList();
        int toplam = puanlar.Sum(x => x.Puan);

        var oneriler = puanlar
            .Take(EnFazlaOneri)
            .Select(x => new BolumOnerisi(x.BolumId, x.Ad, x.Puan, (int)Math.Round(x.Puan * 100.0 / toplam)))
            .ToList();

        // --- 3) Belirsizlik ---
        bool belirsiz = oneriler.Count == 0
                     || oneriler[0].Yuzde < BaskinlikEsigiYuzde
                     || (oneriler.Count > 1 && oneriler[0].Yuzde - oneriler[1].Yuzde < YakinlikEsigiYuzde);

        if (belirsiz && varsayilanBolum is not null && oneriler.All(o => o.BolumId != varsayilanBolum.Id))
            oneriler.Insert(0, new BolumOnerisi(varsayilanBolum.Id, varsayilanBolum.Ad, 0, 0));
        else if (belirsiz && varsayilanBolum is not null)
        {
            // Dahiliye zaten listedeyse başa alınır.
            var d = oneriler.First(o => o.BolumId == varsayilanBolum.Id);
            oneriler.Remove(d);
            oneriler.Insert(0, d);
        }
        if (oneriler.Count > EnFazlaOneri)
            oneriler.RemoveRange(EnFazlaOneri, oneriler.Count - EnFazlaOneri);

        // --- 2) Aciliyet ---
        int enYuksekSiddet = secilenler.Count == 0 ? 0 : secilenler.Max(x => x.Siddet);
        var aciliyet = enYuksekSiddet >= 7 ? AciliyetSeviyesi.Orta : AciliyetSeviyesi.Dusuk;
        var tutanlar = Kurallar.Where(k => k.Kosul(siddetler)).ToList();
        foreach (var k in tutanlar)
            if (k.Seviye > aciliyet)
                aciliyet = k.Seviye;

        var uyarilar = tutanlar.Where(k => k.Seviye == aciliyet).Select(k => k.Uyari).ToList();

        return new YonlendirmeSonucu(aciliyet, uyarilar, oneriler, belirsiz,
            BilgilendirmeOlustur(aciliyet, belirsiz, oneriler.FirstOrDefault()));
    }

    private static List<string> BilgilendirmeOlustur(AciliyetSeviyesi aciliyet, bool belirsiz, BolumOnerisi? enUygun)
    {
        var metin = new List<string>();

        switch (aciliyet)
        {
            case AciliyetSeviyesi.Acil:
                metin.Add("Belirttiğiniz şikâyetler acil değerlendirme gerektirebilir. Randevu beklemeden 112'yi arayın veya en yakın acil servise başvurun.");
                break;
            case AciliyetSeviyesi.Yuksek:
                metin.Add("Şikâyetleriniz kısa sürede değerlendirilmelidir. En yakın tarihli randevuyu alın; belirtileriniz kötüleşirse acil servise başvurun.");
                break;
        }

        if (aciliyet != AciliyetSeviyesi.Acil)
        {
            if (belirsiz)
                metin.Add(enUygun is null
                    ? "Belirttiğiniz şikâyetler için net bir bölüm eşleşmesi bulunamadı."
                    : $"Şikâyetleriniz birden fazla bölüme işaret ediyor veya net bir eşleşme yok. İlk değerlendirme için {enUygun.BolumAdi} bölümüne başvurmanız önerilir; hekiminiz gerekirse sizi ilgili bölüme yönlendirir.");
            else if (enUygun is not null)
                metin.Add($"Şikâyetlerinize en uygun bölüm: {enUygun.BolumAdi}.");
        }

        metin.Add("Bu sonuç bir tıbbi tanı değildir. Yalnızca seçtiğiniz belirtilere göre hangi polikliniğe başvurabileceğiniz konusunda ön yönlendirmedir; kesin değerlendirmeyi hekiminiz yapar.");
        return metin;
    }
}
