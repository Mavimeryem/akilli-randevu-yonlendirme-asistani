using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.DataAccess.Abstract;

public record BolumPuani(int BolumId, string BolumAdi, int ToplamPuan);

public interface IBolumRepository : IRepository<Bolum>
{
    Task<List<Bolum>> GetDoktorlarIleAsync();

    /// <summary>
    /// Seçilen belirtilerin BelirtiBolum ağırlıklarını bölüm bazında toplar, yüksekten düşüğe sıralar.
    /// Bölüm yönlendirme modülünün temel sorgusudur.
    /// </summary>
    Task<List<BolumPuani>> BelirtilereGorePuanlaAsync(IEnumerable<int> belirtiIdleri);
}
