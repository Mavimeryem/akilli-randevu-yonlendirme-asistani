using AkilliRandevu.Business.YapayZeka;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Tests.YapayZeka;

/// <summary>Hafta 10: yapay zekâ çıktısı körlemesine kullanılmaz; her alan doğrulanır.</summary>
public class YzCiktiDogrulayiciTestleri
{
    private static readonly int[] Bolumler = [1, 2, 3, 4, 5, 6, 7, 8];

    [Fact]
    public void Gecerli_yanit_kabul_edilir()
    {
        var (oneri, hata) = YzCiktiDogrulayici.Dogrula(
            """{"bolum_id":6,"gerekce":"Bel ağrısı ve bacağa yayılan ağrı ortopediye işaret ediyor.","guven":"yuksek","acil_belirti_var":false,"acil_gerekce":""}""",
            Bolumler);

        Assert.Null(hata);
        Assert.NotNull(oneri);
        Assert.Equal(6, oneri!.BolumId);
        Assert.Equal(YzGuven.Yuksek, oneri.Guven);
        Assert.False(oneri.AcilBelirtiVar);
        Assert.Null(oneri.AcilGerekce);
    }

    [Theory]
    [InlineData(null, "boş")]
    [InlineData("", "boş")]
    [InlineData("bölüm: ortopedi", "JSON")]
    [InlineData("[1,2]", "nesnesi")]
    [InlineData("""{"gerekce":"yeterince uzun bir gerekçe","guven":"orta","acil_belirti_var":false}""", "bolum_id")]
    [InlineData("""{"bolum_id":99,"gerekce":"yeterince uzun bir gerekçe","guven":"orta","acil_belirti_var":false}""", "izin verilen")]
    [InlineData("""{"bolum_id":2,"gerekce":"kısa","guven":"orta","acil_belirti_var":false}""", "kısa")]
    [InlineData("""{"bolum_id":2,"gerekce":"yeterince uzun bir gerekçe","guven":"cok","acil_belirti_var":false}""", "guven")]
    [InlineData("""{"bolum_id":2,"gerekce":"yeterince uzun bir gerekçe","guven":"orta"}""", "acil_belirti_var")]
    public void Hatali_yanit_reddedilir(string? json, string beklenenHata)
    {
        var (oneri, hata) = YzCiktiDogrulayici.Dogrula(json, Bolumler);

        Assert.Null(oneri);
        Assert.Contains(beklenenHata, hata);
    }

    [Fact]
    public void Acil_isaretli_ama_gerekcesiz_yanita_varsayilan_aciklama_konur()
    {
        var (oneri, _) = YzCiktiDogrulayici.Dogrula(
            """{"bolum_id":2,"gerekce":"Göğüs ağrısı kardiyolojiye işaret ediyor.","guven":"orta","acil_belirti_var":true,"acil_gerekce":""}""",
            Bolumler);

        Assert.True(oneri!.AcilBelirtiVar);
        Assert.False(string.IsNullOrWhiteSpace(oneri.AcilGerekce));
    }

    [Fact]
    public void Cok_uzun_gerekce_kisaltilir()
    {
        var uzun = new string('a', 2000);
        var (oneri, _) = YzCiktiDogrulayici.Dogrula(
            $$"""{"bolum_id":1,"gerekce":"{{uzun}}","guven":"dusuk","acil_belirti_var":false,"acil_gerekce":""}""", Bolumler);

        Assert.True(oneri!.Gerekce.Length <= YzCiktiDogrulayici.GerekceEnFazla + 1);
    }
}
