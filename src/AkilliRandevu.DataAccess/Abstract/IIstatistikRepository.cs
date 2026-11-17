using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.DataAccess.Abstract;

/// <summary>İstatistik ve pano için hafif randevu satırı (yalnızca gereken sütunlar seçilir).</summary>
public record RandevuOzeti(
    int Id,
    DateTime Baslangic,
    DateTime Bitis,
    RandevuDurumu Durum,
    int DoktorId,
    string DoktorAdi,
    int BolumId,
    string BolumAdi,
    string HastaAdi,
    string HastaSoyadi);

public record DoktorOzeti(int DoktorId, string DoktorAdi, int BolumId, string BolumAdi);

public interface IIstatistikRepository
{
    /// <summary>[baslangic, bitis) aralığında başlayan tüm randevular (iptaller dahil), isteğe bağlı bölüm filtresi.</summary>
    Task<List<RandevuOzeti>> GetRandevularAsync(DateTime baslangic, DateTime bitis, int? bolumId);

    /// <summary>Randevu kabul eden doktorlar (kullanıcısı ve bölümü aktif). Kapasite hesabı için.</summary>
    Task<List<DoktorOzeti>> GetAktifDoktorlarAsync(int? bolumId);
}
