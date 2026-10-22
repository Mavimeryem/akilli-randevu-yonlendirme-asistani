namespace AkilliRandevu.Business.Abstract;

public interface ISifreHasher
{
    /// <summary>Şifreyi rastgele tuz ile hash'ler. Sonuç veritabanına yazılacak tek değerdir.</summary>
    string Hashle(string sifre);

    /// <summary>Girilen şifre, kayıtlı hash ile eşleşiyor mu?</summary>
    bool Dogrula(string sifre, string kayitliHash);
}
