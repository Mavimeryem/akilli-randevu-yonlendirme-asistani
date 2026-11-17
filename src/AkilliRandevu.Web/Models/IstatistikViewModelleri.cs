using AkilliRandevu.Business.Istatistik;
using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.Web.Models;

public class IstatistikViewModel
{
    public IstatistikRaporu Rapor { get; set; } = null!;
    public int? BolumId { get; set; }
    public List<Bolum> Bolumler { get; set; } = [];
    public DateOnly Bugun { get; set; }
}

public class SiraPanosuViewModel
{
    public List<DoktorSirasi> Siralar { get; set; } = [];
    public int? BolumId { get; set; }
    public List<Bolum> Bolumler { get; set; } = [];
    public DateTime Simdi { get; set; }
}
