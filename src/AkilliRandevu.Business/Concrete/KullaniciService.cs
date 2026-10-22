using AkilliRandevu.Business.Abstract;
using AkilliRandevu.Business.Dtos;
using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Concrete;

public class KullaniciService : IKullaniciService
{
    public const int MinimumSifreUzunlugu = 8;

    private readonly IKullaniciRepository _kullanicilar;
    private readonly IRepository<Hasta> _hastalar;
    private readonly ISifreHasher _hasher;
    private readonly IGirisDenemeTakipcisi _denemeler;

    // E-posta bulunamadığında da bir hash doğrulaması yapılır; böylece yanıt süresinden
    // "bu e-posta kayıtlı mı" bilgisi sızmaz (kullanıcı adı tahmini / enumeration).
    private readonly Lazy<string> _sahteHash;

    public KullaniciService(
        IKullaniciRepository kullanicilar,
        IRepository<Hasta> hastalar,
        ISifreHasher hasher,
        IGirisDenemeTakipcisi denemeler)
    {
        _kullanicilar = kullanicilar;
        _hastalar = hastalar;
        _hasher = hasher;
        _denemeler = denemeler;
        _sahteHash = new Lazy<string>(() => _hasher.Hashle(Guid.NewGuid().ToString()));
    }

    public static string EpostaNormallestir(string eposta) => eposta.Trim().ToLowerInvariant();

    public async Task<KayitSonucu> HastaKaydetAsync(KayitIstegi istek)
    {
        var hatalar = SifreKurallariniDenetle(istek.Sifre);
        if (istek.TcKimlikNo.Length != 11 || !istek.TcKimlikNo.All(char.IsAsciiDigit))
            hatalar.Add("TC kimlik numarası 11 haneli olmalıdır.");
        if (hatalar.Count > 0)
            return KayitSonucu.Hata([.. hatalar]);

        var eposta = EpostaNormallestir(istek.Eposta);
        if (await _kullanicilar.AnyAsync(k => k.Eposta == eposta))
            return KayitSonucu.Hata("Bu e-posta adresiyle kayıtlı bir hesap var.");
        if (await _hastalar.AnyAsync(h => h.TcKimlikNo == istek.TcKimlikNo))
            return KayitSonucu.Hata("Bu TC kimlik numarasıyla kayıtlı bir hasta var.");

        var kullanici = new Kullanici
        {
            Ad = istek.Ad.Trim(),
            Soyad = istek.Soyad.Trim(),
            Eposta = eposta,
            SifreHash = _hasher.Hashle(istek.Sifre),   // düz şifre hiçbir yere yazılmaz
            Telefon = string.IsNullOrWhiteSpace(istek.Telefon) ? null : istek.Telefon.Trim(),
            Rol = Rol.Hasta,                            // kayıt olan herkes hastadır; rol formdan alınmaz
            Hasta = new Hasta
            {
                TcKimlikNo = istek.TcKimlikNo,
                DogumTarihi = istek.DogumTarihi,
                Cinsiyet = istek.Cinsiyet
            }
        };

        // Kullanici ve Hasta tek SaveChanges ile (tek transaction'da) eklenir: ya ikisi ya hiçbiri.
        await _kullanicilar.AddAsync(kullanici);
        return KayitSonucu.Tamam();
    }

    public async Task<GirisSonucu> GirisYapAsync(string eposta, string sifre)
    {
        eposta = EpostaNormallestir(eposta);

        if (_denemeler.KalanKilitSuresi(eposta) is { } kalan)
            return new GirisSonucu(GirisDurumu.Kilitli, KalanKilitSuresi: kalan);

        var kullanici = await _kullanicilar.GetByEpostaAsync(eposta);
        bool sifreDogru = _hasher.Dogrula(sifre, kullanici?.SifreHash ?? _sahteHash.Value);

        if (kullanici is null || !sifreDogru)
        {
            _denemeler.BasarisizKaydet(eposta);
            if (_denemeler.KalanKilitSuresi(eposta) is { } yeniKilit)
                return new GirisSonucu(GirisDurumu.Kilitli, KalanKilitSuresi: yeniKilit);
            // Hangisinin yanlış olduğu (e-posta mı şifre mi) söylenmez.
            return new GirisSonucu(GirisDurumu.HataliBilgi);
        }

        if (!kullanici.AktifMi)
            return new GirisSonucu(GirisDurumu.HesapPasif);

        _denemeler.Sifirla(eposta);
        return new GirisSonucu(GirisDurumu.Basarili, kullanici);
    }

    public Task<List<Kullanici>> TumKullanicilarAsync() => _kullanicilar.GetTumuProfilleriyleAsync();

    private static List<string> SifreKurallariniDenetle(string sifre)
    {
        var hatalar = new List<string>();
        if (sifre.Length < MinimumSifreUzunlugu)
            hatalar.Add($"Şifre en az {MinimumSifreUzunlugu} karakter olmalıdır.");
        if (!sifre.Any(char.IsLetter) || !sifre.Any(char.IsDigit))
            hatalar.Add("Şifre en az bir harf ve bir rakam içermelidir.");
        return hatalar;
    }
}
