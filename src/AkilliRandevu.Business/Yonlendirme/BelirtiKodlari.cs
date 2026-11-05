namespace AkilliRandevu.Business.Yonlendirme;

/// <summary>
/// Acil durum kurallarında kullanılan belirtilerin Id'leri.
/// Değerler SentetikVeri (migration) içindeki belirti kataloğuyla aynıdır; katalog değişirse burası da güncellenmelidir.
/// </summary>
public static class BelirtiKodlari
{
    public const int BasAgrisi = 1;
    public const int BasDonmesi = 2;
    public const int GogusAgrisi = 3;
    public const int Carpinti = 4;
    public const int NefesDarligi = 5;
    public const int Ates = 7;
    public const int KarinAgrisi = 8;
    public const int BulanikGorme = 12;
    public const int Uyusma = 16;
}
