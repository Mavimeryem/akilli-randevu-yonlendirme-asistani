using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.DataAccess.Abstract;

public interface IKullaniciRepository : IRepository<Kullanici>
{
    /// <summary>Giriş için: kullanıcıyı hasta/doktor profiliyle birlikte getirir.</summary>
    Task<Kullanici?> GetByEpostaAsync(string eposta);

    Task<List<Kullanici>> GetTumuProfilleriyleAsync();
}
