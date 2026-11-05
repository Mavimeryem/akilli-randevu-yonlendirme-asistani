using AkilliRandevu.Entities.Concrete;

namespace AkilliRandevu.DataAccess.Abstract;

public interface IBelirtiKaydiRepository : IRepository<BelirtiKaydi>
{
    /// <summary>Kayıt; seçilen belirtiler, önerilen bölüm ve bağlı randevu ile.</summary>
    Task<BelirtiKaydi?> GetDetayliAsync(int kayitId);

    /// <summary>Hasta geçmişi: hastanın belirti kayıtları, yeniden eskiye.</summary>
    Task<List<BelirtiKaydi>> GetHastaKayitlariAsync(int hastaId);

    /// <summary>Hastanın verilen andan sonra oluşturduğu kayıt sayısı (günlük sınır için).</summary>
    Task<int> SayAsync(int hastaId, DateTime baslangic);
}
