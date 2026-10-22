using AkilliRandevu.Business.Abstract;
using Microsoft.Extensions.Caching.Memory;

namespace AkilliRandevu.Business.Concrete;

/// <summary>
/// Aynı e-postaya 5 başarısız denemeden sonra girişi 15 dakika kilitler.
/// Sayaçlar bellekte tutulur (uygulama yeniden başlarsa sıfırlanır); tek sunuculu bu proje için yeterlidir.
/// </summary>
public sealed class BellekGirisDenemeTakipcisi : IGirisDenemeTakipcisi
{
    public const int MaksimumDeneme = 5;
    public static readonly TimeSpan KilitSuresi = TimeSpan.FromMinutes(15);

    private readonly IMemoryCache _cache;
    private readonly TimeProvider _zaman;
    private readonly object _kilit = new();

    public BellekGirisDenemeTakipcisi(IMemoryCache cache, TimeProvider zaman)
    {
        _cache = cache;
        _zaman = zaman;
    }

    private sealed class Sayac
    {
        public int Deneme;
        public DateTimeOffset? KilitBitisi;
    }

    private static string Anahtar(string eposta) => "giris-deneme:" + eposta;

    public TimeSpan? KalanKilitSuresi(string eposta)
    {
        if (_cache.TryGetValue(Anahtar(eposta), out Sayac? s) && s!.KilitBitisi is { } bitis)
        {
            var kalan = bitis - _zaman.GetUtcNow();
            if (kalan > TimeSpan.Zero)
                return kalan;
        }
        return null;
    }

    public void BasarisizKaydet(string eposta)
    {
        lock (_kilit)
        {
            var s = _cache.GetOrCreate(Anahtar(eposta), e =>
            {
                e.SlidingExpiration = KilitSuresi;
                return new Sayac();
            })!;

            // Önceki kilit dolmuşsa sayaç yeniden başlar.
            if (s.KilitBitisi is { } eski && eski <= _zaman.GetUtcNow())
            {
                s.Deneme = 0;
                s.KilitBitisi = null;
            }

            s.Deneme++;
            if (s.Deneme >= MaksimumDeneme)
                s.KilitBitisi = _zaman.GetUtcNow().Add(KilitSuresi);
        }
    }

    public void Sifirla(string eposta) => _cache.Remove(Anahtar(eposta));
}
