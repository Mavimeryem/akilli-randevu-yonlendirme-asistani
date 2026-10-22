using System.Security.Cryptography;
using AkilliRandevu.Business.Abstract;

namespace AkilliRandevu.Business.Concrete;

/// <summary>
/// PBKDF2-HMAC-SHA256 ile şifre hash'leme (OWASP önerisi: 600.000 tekrar).
/// Saklanan biçim: PBKDF2-SHA256$tekrar$tuz(base64)$hash(base64)
///
/// - Hash tek yönlüdür: hash'ten şifre geri elde edilemez.
/// - Tuz (salt) her kullanıcı için rastgele üretilir; aynı şifreye sahip iki kullanıcının
///   hash'i farklı çıkar ve önceden hesaplanmış tablolar (rainbow table) işe yaramaz.
/// - Yüksek tekrar sayısı her denemeyi yavaşlatır; kaba kuvvet saldırısı pahalılaşır.
/// </summary>
public sealed class Pbkdf2SifreHasher : ISifreHasher
{
    private const string Onek = "PBKDF2-SHA256";
    private const int TuzBoyutu = 16;     // 128 bit
    private const int HashBoyutu = 32;    // 256 bit
    private const int Tekrar = 600_000;
    private static readonly HashAlgorithmName Algoritma = HashAlgorithmName.SHA256;

    public string Hashle(string sifre)
    {
        byte[] tuz = RandomNumberGenerator.GetBytes(TuzBoyutu);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(sifre, tuz, Tekrar, Algoritma, HashBoyutu);
        return $"{Onek}${Tekrar}${Convert.ToBase64String(tuz)}${Convert.ToBase64String(hash)}";
    }

    public bool Dogrula(string sifre, string kayitliHash)
    {
        var parcalar = kayitliHash.Split('$');
        if (parcalar.Length != 4 || parcalar[0] != Onek || !int.TryParse(parcalar[1], out int tekrar))
            return false;

        byte[] tuz, beklenen;
        try
        {
            tuz = Convert.FromBase64String(parcalar[2]);
            beklenen = Convert.FromBase64String(parcalar[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        // Aynı tuz ve tekrar sayısıyla girilen şifre yeniden hash'lenir ve karşılaştırılır.
        byte[] hesaplanan = Rfc2898DeriveBytes.Pbkdf2(sifre, tuz, tekrar, Algoritma, beklenen.Length);

        // Sabit zamanlı karşılaştırma: kaç baytın eşleştiği süreden anlaşılamaz (timing saldırısı).
        return CryptographicOperations.FixedTimeEquals(hesaplanan, beklenen);
    }
}
