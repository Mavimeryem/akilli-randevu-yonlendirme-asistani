using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Yonlendirme;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Concrete;

public class BelirtiService : IBelirtiService
{
    public const int EnAzBelirti = 1;
    public const int EnFazlaBelirti = 8;
    public const int AciklamaUzunlugu = 1000;
    public const int GunlukKayitSiniri = 10;
    public const string VarsayilanBolumAdi = "Dahiliye";

    private readonly IRepository<Belirti> _belirtiler;
    private readonly IBelirtiKaydiRepository _kayitlar;
    private readonly IBolumRepository _bolumler;
    private readonly IRepository<Hasta> _hastalar;
    private readonly TimeProvider _zaman;

    public BelirtiService(
        IRepository<Belirti> belirtiler,
        IBelirtiKaydiRepository kayitlar,
        IBolumRepository bolumler,
        IRepository<Hasta> hastalar,
        TimeProvider zaman)
    {
        _belirtiler = belirtiler;
        _kayitlar = kayitlar;
        _bolumler = bolumler;
        _hastalar = hastalar;
        _zaman = zaman;
    }

    public async Task<List<Belirti>> BelirtileriGetirAsync() =>
        (await _belirtiler.GetAllAsync()).OrderBy(b => b.Ad).ToList();

    public async Task<IslemSonucu<int>> DegerlendirAsync(int hastaId, IReadOnlyList<SecilenBelirti> belirtiler, string? aciklama)
    {
        // --- Doğrulama ---
        if (belirtiler.Count < EnAzBelirti)
            return Hata("En az bir belirti seçmelisiniz.");
        if (belirtiler.Count > EnFazlaBelirti)
            return Hata($"En fazla {EnFazlaBelirti} belirti seçebilirsiniz. En belirgin şikâyetlerinizi seçin.");
        if (belirtiler.Select(b => b.BelirtiId).Distinct().Count() != belirtiler.Count)
            return Hata("Aynı belirti birden fazla kez seçilemez.");
        if (belirtiler.Any(b => b.Siddet is < 1 or > 10))
            return Hata("Şiddet 1 ile 10 arasında olmalıdır.");

        aciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim();
        if (aciklama?.Length > AciklamaUzunlugu)
            return Hata($"Açıklama en fazla {AciklamaUzunlugu} karakter olabilir.");

        var idler = belirtiler.Select(b => b.BelirtiId).ToList();
        var katalog = await _belirtiler.WhereAsync(b => idler.Contains(b.Id));
        if (katalog.Count != idler.Count)
            return Hata("Geçersiz belirti seçimi.");

        if (await _hastalar.GetByIdAsync(hastaId) is null)
            return Hata("Hasta kaydı bulunamadı.");

        // Kötüye kullanım sınırı: son 24 saatte en fazla 10 kayıt.
        var birGunOnce = _zaman.GetUtcNow().UtcDateTime.AddDays(-1);
        if (await _kayitlar.SayAsync(hastaId, birGunOnce) >= GunlukKayitSiniri)
            return Hata("Son 24 saatte çok fazla belirti girişi yaptınız. Lütfen daha sonra tekrar deneyin.");

        // --- Yönlendirme ---
        var sonuc = await HesaplaAsync(belirtiler);

        // --- Kayıt: BelirtiKaydi + BelirtiKaydiBelirti satırları tek SaveChanges ile eklenir ---
        var kayit = await _kayitlar.AddAsync(new BelirtiKaydi
        {
            HastaId = hastaId,
            Aciklama = aciklama,
            Aciliyet = sonuc.Aciliyet,
            OnerilenBolumId = sonuc.AcilMi ? null : sonuc.EnUygun?.BolumId,
            Belirtiler = belirtiler.Select(b => new BelirtiKaydiBelirti { BelirtiId = b.BelirtiId, Siddet = b.Siddet }).ToList()
        });
        return IslemSonucu<int>.Tamam(kayit.Id);
    }

    public async Task<IslemSonucu<BelirtiKaydiDetayi>> KayitGetirAsync(int kayitId, Islemci kim)
    {
        var kayit = await _kayitlar.GetDetayliAsync(kayitId);

        // Sahiplik: hasta yalnızca kendi kaydını görür. Kayıt yoksa da aynı mesaj verilir (Id tahmini bilgi sızdırmasın).
        bool erisebilir = kayit is not null && (kim.Rol == Rol.Yonetici || (kim.Rol == Rol.Hasta && kim.HastaId == kayit.HastaId));
        if (!erisebilir)
            return IslemSonucu<BelirtiKaydiDetayi>.Hatali("Belirti kaydı bulunamadı.");

        var sonuc = await HesaplaAsync(kayit!.Belirtiler.Select(b => new SecilenBelirti(b.BelirtiId, b.Siddet)).ToList());
        return IslemSonucu<BelirtiKaydiDetayi>.Tamam(new BelirtiKaydiDetayi(kayit, sonuc));
    }

    public Task<List<BelirtiKaydi>> HastaKayitlariAsync(int hastaId) => _kayitlar.GetHastaKayitlariAsync(hastaId);

    private async Task<YonlendirmeSonucu> HesaplaAsync(IReadOnlyList<SecilenBelirti> belirtiler)
    {
        var eslesmeler = await _bolumler.GetEslesmelerAsync(belirtiler.Select(b => b.BelirtiId));
        var varsayilan = (await _bolumler.WhereAsync(b => b.Ad == VarsayilanBolumAdi && b.AktifMi)).FirstOrDefault();
        return YonlendirmeMotoru.Degerlendir(belirtiler, eslesmeler, varsayilan);
    }

    private static IslemSonucu<int> Hata(string mesaj) => IslemSonucu<int>.Hatali(mesaj);
}
