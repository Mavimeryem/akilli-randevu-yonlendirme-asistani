using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.DataAccess.Abstract;

public interface IDoktorRepository : IRepository<Doktor>
{
    /// <summary>Doktor, kullanıcı ve bölüm bilgisiyle.</summary>
    Task<Doktor?> GetDetayliAsync(int doktorId);

    /// <summary>Bölümdeki aktif doktorlar (kullanıcısı aktif olanlar).</summary>
    Task<List<Doktor>> GetBolumdekilerAsync(int bolumId);
}
