using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Gecmis;

public interface IHastaGecmisiService
{
    /// <summary>
    /// Hastanın geçmişini filtreleyerek getirir. Erişim kuralı ihlalinde "bulunamadı" döner.
    /// Hasta dışında biri eriştiğinde erişim izi kaydedilir.
    /// </summary>
    Task<IslemSonucu<HastaGecmisi>> GetirAsync(int hastaId, GecmisFiltresi filtre, Islemci kim, int kullaniciId);

    /// <summary>Hastanın geçmişine kimlerin eriştiği. Hasta yalnızca kendisininkini, yönetici herkesinkini görür.</summary>
    Task<IslemSonucu<List<ErisimKaydi>>> ErisimKayitlariAsync(int? hastaId, Islemci kim, int adet = 50);

    /// <summary>Doktorun randevu ilişkisi olan hastaları.</summary>
    Task<List<DoktorHastasi>> DoktorHastalariAsync(int doktorId);
}

/// <summary>
/// Hasta geçmişi modülü.
///
/// Erişim kuralları (kim kimin geçmişini görebilir):
///   Hasta    → yalnızca kendi geçmişi
///   Doktor   → yalnızca en az bir randevusu olan (tedavi ilişkisi bulunan) hastaların geçmişi
///   Yönetici → tüm hastalar, ama tıbbi içerik (açıklama, doktor notu, YZ gerekçesi, yaş, cinsiyet) gizli
/// Yetkisiz istekte "bulunamadı" denir: hastanın sistemde olup olmadığı bile sızmaz.
/// </summary>
public class HastaGecmisiService : IHastaGecmisiService
{
    public const int SayfaBoyutu = 20;
    public const int EnFazlaAralikGun = 731; // 2 yıl

    private readonly IHastaGecmisiRepository _repo;
    private readonly ISaat _saat;

    public HastaGecmisiService(IHastaGecmisiRepository repo, ISaat saat)
    {
        _repo = repo;
        _saat = saat;
    }

    public static string? FiltreyiDenetle(GecmisFiltresi f)
    {
        if (f.Baslangic is { } b && f.Bitis is { } s)
        {
            if (s < b)
                return "Bitiş tarihi başlangıç tarihinden önce olamaz.";
            if (s.DayNumber - b.DayNumber > EnFazlaAralikGun)
                return "Tarih aralığı en fazla 2 yıl olabilir.";
        }
        if (f.Sayfa < 1)
            return "Sayfa numarası geçersiz.";
        if (f.Tur == GecmisTuru.Belirti && f.Durum is not null)
            return "Durum filtresi yalnızca randevular için kullanılabilir.";
        return null;
    }

    public async Task<bool> ErisebilirMiAsync(int hastaId, Islemci kim) => kim.Rol switch
    {
        Rol.Hasta => kim.HastaId == hastaId,
        Rol.Doktor => kim.DoktorId is int d && await _repo.DoktorHastaIliskisiVarMiAsync(d, hastaId),
        Rol.Yonetici => true,
        _ => false
    };

    public async Task<IslemSonucu<HastaGecmisi>> GetirAsync(int hastaId, GecmisFiltresi filtre, Islemci kim, int kullaniciId)
    {
        if (FiltreyiDenetle(filtre) is string hata)
            return IslemSonucu<HastaGecmisi>.Hatali(hata);

        var hasta = await _repo.GetHastaAsync(hastaId);
        if (hasta is null || !await ErisebilirMiAsync(hastaId, kim))
            return IslemSonucu<HastaGecmisi>.Hatali("Hasta geçmişi bulunamadı.");

        // Erişim izi: hastanın kendisi dışındaki her görüntüleme kaydedilir.
        if (kim.Rol != Rol.Hasta)
            await _repo.ErisimKaydetAsync(new ErisimKaydi
            {
                HastaId = hastaId,
                ErisenKullaniciId = kullaniciId,
                ErisenRol = kim.Rol,
                Islem = "Hasta geçmişi görüntülendi"
            });

        bool gizli = kim.Rol == Rol.Yonetici;

        // Tarih filtresi: randevular yerel saatle, belirti kayıtları UTC ile tutulur.
        DateTime? basYerel = filtre.Baslangic?.ToDateTime(TimeOnly.MinValue);
        DateTime? bitYerel = filtre.Bitis?.AddDays(1).ToDateTime(TimeOnly.MinValue); // bitiş günü dahil

        var olaylar = new List<GecmisOlayi>();

        var randevular = filtre.Tur == GecmisTuru.Belirti
            ? []
            : await _repo.GetRandevularAsync(hastaId, basYerel, bitYerel, filtre.Durum, filtre.BolumId);
        olaylar.AddRange(randevular.Select(r => RandevuOlayi(r, kim, gizli)));

        if (filtre.Tur != GecmisTuru.Randevu && filtre.Durum is null)
        {
            var kayitlar = await _repo.GetBelirtiKayitlariAsync(hastaId,
                basYerel is { } b ? _saat.UtcyeCevir(b) : null,
                bitYerel is { } s ? _saat.UtcyeCevir(s) : null,
                filtre.BolumId);
            olaylar.AddRange(kayitlar.Select(k => BelirtiOlayi(k, gizli)));
        }

        olaylar = olaylar.OrderByDescending(o => o.Tarih).ThenByDescending(o => o.KayitId).ToList();

        int toplamSayfa = Math.Max(1, (int)Math.Ceiling(olaylar.Count / (double)SayfaBoyutu));
        int sayfa = Math.Min(filtre.Sayfa, toplamSayfa);
        var sayfadaki = olaylar.Skip((sayfa - 1) * SayfaBoyutu).Take(SayfaBoyutu).ToList();

        // Özet her zaman filtresiz tüm geçmişten hesaplanır.
        var tumRandevular = await _repo.GetRandevularAsync(hastaId, null, null, null, null);
        var tumKayitSayisi = (await _repo.GetBelirtiKayitlariAsync(hastaId, null, null, null)).Count;
        var simdi = _saat.Simdi;
        var ozet = new GecmisOzeti(
            tumRandevular.Count,
            tumRandevular.Count(r => r.Durum == RandevuDurumu.Tamamlandi),
            tumRandevular.Count(r => r.Durum == RandevuDurumu.IptalEdildi),
            tumRandevular.Count(r => r.Durum == RandevuDurumu.Gelmedi),
            tumKayitSayisi,
            tumRandevular.Where(r => r.Durum == RandevuDurumu.Tamamlandi && r.BaslangicZamani <= simdi)
                         .Select(r => (DateTime?)r.BaslangicZamani).Max());

        var k = hasta.Kullanici;
        return IslemSonucu<HastaGecmisi>.Tamam(new HastaGecmisi(
            hastaId,
            $"{k.Ad} {k.Soyad}",
            gizli ? null : Yas(hasta.DogumTarihi, DateOnly.FromDateTime(simdi)),
            gizli ? null : hasta.Cinsiyet,
            ozet,
            sayfadaki,
            sayfa,
            toplamSayfa,
            olaylar.Count,
            gizli));
    }

    private static GecmisOlayi RandevuOlayi(Randevu r, Islemci kim, bool gizli) => new(
        GecmisTuru.Randevu,
        r.Id,
        r.BaslangicZamani,
        $"{r.Doktor.Bolum.Ad} randevusu",
        r.Doktor.Bolum.Ad,
        $"{r.Doktor.Unvan} {r.Doktor.Kullanici.Ad} {r.Doktor.Kullanici.Soyad}",
        r.Durum,
        null,
        [],
        null,
        gizli ? null : r.DoktorNotu,
        null,
        kim.Rol == Rol.Doktor && kim.DoktorId == r.DoktorId);

    private GecmisOlayi BelirtiOlayi(BelirtiKaydi k, bool gizli)
    {
        string? yz = null;
        if (!gizli && k.YzDegerlendirmesi is { Durum: YzDurumu.Basarili, OnerilenBolum: { } yzBolum } y)
            yz = $"Yapay zekâ önerisi: {yzBolum.Ad} (güven: {y.Guven switch { YzGuven.Yuksek => "yüksek", YzGuven.Orta => "orta", _ => "düşük" }})";

        return new GecmisOlayi(
            GecmisTuru.Belirti,
            k.Id,
            _saat.YereleCevir(k.OlusturmaTarihi),
            "Belirti girişi",
            k.OnerilenBolum?.Ad,
            null,
            null,
            k.Aciliyet,
            gizli ? [] : k.Belirtiler.OrderByDescending(b => b.Siddet).Select(b => $"{b.Belirti.Ad} ({b.Siddet}/10)").ToList(),
            gizli ? null : k.Aciklama,
            null,
            yz,
            false);
    }

    private static int Yas(DateOnly dogum, DateOnly bugun)
    {
        int yas = bugun.Year - dogum.Year;
        if (bugun < dogum.AddYears(yas)) yas--;
        return yas;
    }

    public async Task<IslemSonucu<List<ErisimKaydi>>> ErisimKayitlariAsync(int? hastaId, Islemci kim, int adet = 50)
    {
        bool izin = kim.Rol switch
        {
            Rol.Hasta => hastaId is not null && kim.HastaId == hastaId,
            Rol.Yonetici => true,
            _ => false
        };
        if (!izin)
            return IslemSonucu<List<ErisimKaydi>>.Hatali("Erişim kayıtlarını görme yetkiniz yok.");
        return IslemSonucu<List<ErisimKaydi>>.Tamam(await _repo.GetErisimKayitlariAsync(hastaId, adet));
    }

    public Task<List<DoktorHastasi>> DoktorHastalariAsync(int doktorId) => _repo.GetDoktorHastalariAsync(doktorId);
}
