using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.DataAccess.Abstract;

public interface IBildirimRepository : IRepository<Bildirim>
{
    Task<List<Bildirim>> GetKullaniciBildirimleriAsync(int kullaniciId, int adet);

    Task<int> OkunmamisSayisiAsync(int kullaniciId);

    /// <summary>Kullanıcının bildirimlerini okundu yapar; bildirimId verilirse yalnızca onu.</summary>
    Task OkunduYapAsync(int kullaniciId, int? bildirimId);

    /// <summary>Giden kutusu: gönderilmeyi bekleyen e-postalar (bildirimiyle, değişiklik takibi açık), en eskiden.</summary>
    Task<List<EpostaGonderimi>> GetBekleyenEpostalarAsync(int adet);

    Task UpdateEpostaAsync(EpostaGonderimi gonderim);

    /// <summary>Yönetici ekranı: son e-posta gönderim kayıtları.</summary>
    Task<List<EpostaGonderimi>> GetSonEpostalarAsync(int adet);
}
