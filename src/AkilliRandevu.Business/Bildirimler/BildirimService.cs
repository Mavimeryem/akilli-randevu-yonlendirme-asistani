using AkilliRandevu.DataAccess.Abstract;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.Bildirimler;

public interface IBildirimService
{
    /// <summary>
    /// Uygulama içi bildirim oluşturur ve e-posta kanalı için giden kutusuna kayıt ekler.
    /// E-posta burada GÖNDERİLMEZ (veritabanı işlemi dış API'yi beklemesin); arka plan servisi gönderir.
    /// </summary>
    Task OlusturAsync(int kullaniciId, string baslik, string mesaj);

    Task<List<Bildirim>> KullaniciBildirimleriAsync(int kullaniciId, int adet = 50);

    Task<int> OkunmamisSayisiAsync(int kullaniciId);

    Task OkunduYapAsync(int kullaniciId, int? bildirimId = null);

    Task<List<EpostaGonderimi>> SonEpostaGonderimleriAsync(int adet = 100);
}

public class BildirimService : IBildirimService
{
    private readonly IBildirimRepository _bildirimler;
    private readonly IKullaniciRepository _kullanicilar;
    private readonly EpostaAyarlari _eposta;

    public BildirimService(IBildirimRepository bildirimler, IKullaniciRepository kullanicilar, IOptions<EpostaAyarlari> eposta)
    {
        _bildirimler = bildirimler;
        _kullanicilar = kullanicilar;
        _eposta = eposta.Value;
    }

    public async Task OlusturAsync(int kullaniciId, string baslik, string mesaj)
    {
        var kullanici = await _kullanicilar.GetByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException($"Kullanıcı bulunamadı: {kullaniciId}");

        var bildirim = new Bildirim { KullaniciId = kullaniciId, Baslik = baslik, Mesaj = mesaj };

        // Pasif kullanıcıya e-posta gönderilmez; servis ayarlı değilse kayıt "Atlandı" olarak tutulur.
        if (kullanici.AktifMi)
        {
            bildirim.EpostaGonderimleri.Add(new EpostaGonderimi
            {
                Alici = kullanici.Eposta,
                Durum = _eposta.Yapilandirildi ? GonderimDurumu.Bekliyor : GonderimDurumu.Atlandi,
                SonHata = _eposta.Yapilandirildi ? null : "E-posta servisi yapılandırılmamış."
            });
        }

        await _bildirimler.AddAsync(bildirim);
    }

    public Task<List<Bildirim>> KullaniciBildirimleriAsync(int kullaniciId, int adet = 50) =>
        _bildirimler.GetKullaniciBildirimleriAsync(kullaniciId, adet);

    public Task<int> OkunmamisSayisiAsync(int kullaniciId) => _bildirimler.OkunmamisSayisiAsync(kullaniciId);

    public Task OkunduYapAsync(int kullaniciId, int? bildirimId = null) =>
        _bildirimler.OkunduYapAsync(kullaniciId, bildirimId);

    public Task<List<EpostaGonderimi>> SonEpostaGonderimleriAsync(int adet = 100) =>
        _bildirimler.GetSonEpostalarAsync(adet);
}
