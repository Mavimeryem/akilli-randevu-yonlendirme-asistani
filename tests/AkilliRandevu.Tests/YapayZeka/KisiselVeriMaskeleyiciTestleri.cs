using AkilliRandevu.Business.YapayZeka;

namespace AkilliRandevu.Tests.YapayZeka;

/// <summary>Hafta 10 (KVKK): dış servise gönderilen metinde kimlik belirleyici bilgi kalmamalı.</summary>
public class KisiselVeriMaskeleyiciTestleri
{
    [Theory]
    [InlineData("TC'm 12345678901, yardım edin", "12345678901")]
    [InlineData("tc: 123 456 789 01", "123 456 789 01")]
    [InlineData("beni 0532 123 45 67 numaradan arayın", "0532 123 45 67")]
    [InlineData("tel +90 555 000 11 22", "555 000 11 22")]
    [InlineData("telefon (0505) 123-4567", "123-4567")]
    [InlineData("mail: ali.veli@ornek.com", "ali.veli@ornek.com")]
    public void Kimlik_telefon_ve_eposta_maskelenir(string metin, string gizlenmeli)
    {
        var sonuc = KisiselVeriMaskeleyici.Maskele(metin);

        Assert.DoesNotContain(gizlenmeli, sonuc);
        Assert.Contains("[", sonuc);
    }

    [Fact]
    public void Belirti_metni_degismeden_kalir()
    {
        const string metin = "3 gündür başım ağrıyor, ateşim 38,5 derece, 2 kez kustum.";

        Assert.Equal(metin, KisiselVeriMaskeleyici.Maskele(metin));
    }
}
