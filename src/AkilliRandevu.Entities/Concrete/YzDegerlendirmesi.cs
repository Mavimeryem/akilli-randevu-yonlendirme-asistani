using AkilliRandevu.Entities.Common;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Entities.Concrete;

/// <summary>
/// Bir belirti kaydının yapay zekâ (Claude) ile değerlendirilmesi. BelirtiKaydi 1-0..1 YzDegerlendirmesi.
/// Başarısız denemeler de kaydedilir: model, istem sürümü, süre ve hata birlikte tutulur (yapay zekâ günlüğü).
/// </summary>
public class YzDegerlendirmesi : BaseEntity
{
    public int BelirtiKaydiId { get; set; }
    public YzDurumu Durum { get; set; }

    public int? OnerilenBolumId { get; set; }
    public string? Gerekce { get; set; }
    public YzGuven? Guven { get; set; }
    public bool AcilBelirtiVar { get; set; }
    public string? AcilGerekce { get; set; }

    public string Model { get; set; } = null!;
    public string IstemSurumu { get; set; } = null!;
    public int SureMs { get; set; }
    public string? Hata { get; set; }

    public BelirtiKaydi BelirtiKaydi { get; set; } = null!;
    public Bolum? OnerilenBolum { get; set; }
}
