namespace AkilliRandevu.Business.Abstract;

/// <summary>Kaba kuvvet (brute force) saldırısına karşı başarısız giriş denemelerini sayar.</summary>
public interface IGirisDenemeTakipcisi
{
    /// <summary>Hesap kilitliyse kalan süreyi, değilse null döner.</summary>
    TimeSpan? KalanKilitSuresi(string eposta);

    void BasarisizKaydet(string eposta);

    void Sifirla(string eposta);
}
