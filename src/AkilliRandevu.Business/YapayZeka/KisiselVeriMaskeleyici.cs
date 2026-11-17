using System.Text.RegularExpressions;

namespace AkilliRandevu.Business.YapayZeka;

/// <summary>
/// Hastanın serbest metni dış servise gönderilmeden önce kimlik belirleyici bilgileri maskeler (KVKK – veri minimizasyonu).
/// Ad, yaş, TC gibi profil bilgileri zaten hiç gönderilmez; bu sınıf hastanın metne kendisinin yazdıklarını yakalar.
/// </summary>
public static partial class KisiselVeriMaskeleyici
{
    [GeneratedRegex(@"[\w.+-]+@[\w-]+(\.[\w-]+)+")]
    private static partial Regex Eposta();

    // 11 haneli TC kimlik numarası (aralarında boşluk olabilir)
    [GeneratedRegex(@"\b\d(?:\s?\d){10}\b")]
    private static partial Regex TcKimlik();

    // Türkiye telefonları: +90 / 0 ile başlayan 10 haneli numaralar, boşluk/tire/parantezli yazımlar
    [GeneratedRegex(@"(?:\+?90[\s-]?)?\(?0?5\d{2}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}")]
    private static partial Regex Telefon();

    public static string Maskele(string metin)
    {
        var sonuc = Eposta().Replace(metin, "[e-posta]");
        sonuc = Telefon().Replace(sonuc, "[telefon]");
        sonuc = TcKimlik().Replace(sonuc, "[kimlik no]");
        return sonuc;
    }
}
