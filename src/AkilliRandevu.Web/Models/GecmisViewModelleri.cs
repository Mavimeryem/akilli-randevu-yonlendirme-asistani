using AkilliRandevu.Business.Gecmis;
using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.Web.Models;

public class GecmisViewModel
{
    public HastaGecmisi Gecmis { get; set; } = null!;
    public GecmisFiltresi Filtre { get; set; } = new();
    public List<Bolum> Bolumler { get; set; } = [];

    /// <summary>Hasta kendi geçmişine mi bakıyor (erişim kayıtları yalnızca o zaman gösterilir).</summary>
    public bool KendiGecmisi { get; set; }
    public List<ErisimKaydi> ErisimKayitlari { get; set; } = [];

    public DateTime Simdi { get; set; }
}
