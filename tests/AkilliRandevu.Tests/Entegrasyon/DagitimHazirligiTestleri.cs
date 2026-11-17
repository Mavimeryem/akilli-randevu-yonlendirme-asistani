using System.Net;

namespace AkilliRandevu.Tests.Entegrasyon;

/// <summary>
/// Hafta 14 – dağıtım hazırlığı ve kullanılabilirlik: sağlık kontrolü, Türkçe hata sayfası,
/// gizlilik metni, demo uyarısı ve menü erişilebilirliği.
/// </summary>
public class DagitimHazirligiTestleri : IClassFixture<WebFabrikasi>
{
    private readonly WebFabrikasi _f;

    public DagitimHazirligiTestleri(WebFabrikasi f)
    {
        _f = f;
    }

    [Fact]
    public async Task Saglik_kontrolu_Healthy_doner()
    {
        var y = await _f.Istemci().GetAsync("/saglik");

        Assert.Equal(HttpStatusCode.OK, y.StatusCode);
        Assert.Equal("Healthy", await y.MetinAsync());
    }

    [Fact]
    public async Task Olmayan_sayfa_404_ve_Turkce_hata_sayfasi_doner()
    {
        var y = await _f.Istemci().GetAsync("/olmayan-sayfa");

        Assert.Equal(HttpStatusCode.NotFound, y.StatusCode);
        Assert.Contains("Sayfa bulunamadı", await y.MetinAsync());
    }

    [Fact]
    public async Task Gizlilik_sayfasi_KVKK_metnini_icerir()
    {
        var y = await _f.Istemci().GetAsync("/Home/Gizlilik");

        Assert.Equal(HttpStatusCode.OK, y.StatusCode);
        Assert.Contains("KVKK", await y.MetinAsync());
    }

    [Fact]
    public async Task Demo_modunda_sayfa_altinda_uyari_gorunur()
    {
        var metin = await (await _f.Istemci().GetAsync("/")).MetinAsync();

        Assert.Contains("Demo sürümü", metin);
    }

    [Fact]
    public async Task Sayfada_icerige_gec_baglantisi_ve_aktif_menu_isareti_var()
    {
        var c = _f.Istemci();
        await c.GirisYapAsync("hasta1@ornek.test");

        var metin = await (await c.GetAsync("/Hasta/Randevularim")).MetinAsync();

        Assert.Contains("href=\"#icerik\"", metin);
        Assert.Contains("aria-current=\"page\"", metin);
    }

    [Fact]
    public async Task Yonetici_menusunde_yonetim_baglantilari_var_hastada_yok()
    {
        var y = _f.Istemci();
        await y.GirisYapAsync("yonetici@ornek.test");
        var h = _f.Istemci();
        await h.GirisYapAsync("hasta1@ornek.test");

        Assert.Contains("/Yonetim/EpostaGonderimleri", await (await y.GetAsync("/")).MetinAsync());
        Assert.DoesNotContain("/Yonetim/EpostaGonderimleri", await (await h.GetAsync("/")).MetinAsync());
    }
}
