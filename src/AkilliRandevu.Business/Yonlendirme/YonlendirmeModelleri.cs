using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Yonlendirme;

/// <summary>Hastanın seçtiği bir belirti ve şiddeti (1-10).</summary>
public record SecilenBelirti(int BelirtiId, int Siddet);

public record BolumOnerisi(int BolumId, string BolumAdi, int Puan, int Yuzde);

public record YonlendirmeSonucu(
    AciliyetSeviyesi Aciliyet,
    IReadOnlyList<string> AcilUyarilari,
    IReadOnlyList<BolumOnerisi> Oneriler,
    bool Belirsiz,
    IReadOnlyList<string> Bilgilendirme)
{
    public BolumOnerisi? EnUygun => Oneriler.Count > 0 ? Oneriler[0] : null;
    public bool AcilMi => Aciliyet == AciliyetSeviyesi.Acil;
}
