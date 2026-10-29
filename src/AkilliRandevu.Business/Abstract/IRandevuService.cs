using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Business.Takvim;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Abstract;

public interface IRandevuService
{
    Task<List<Randevu>> HastaGecmisiAsync(int hastaId);

    Task<List<Randevu>> DoktorGunlukAsync(int doktorId, DateOnly gun);

    Task<List<Randevu>> GunlukTumuAsync(DateOnly gun, int? bolumId);

    /// <summary>Doktorun o günkü slotları (boş / dolu / geçmiş).</summary>
    Task<List<Slot>> SlotlariGetirAsync(int doktorId, DateOnly gun);

    /// <summary>Randevu alma ekranı için: aktif bölümler ve içlerindeki aktif doktorlar.</summary>
    Task<List<Bolum>> BolumleriDoktorlariylaAsync();

    /// <summary>Tüm iş kurallarını denetleyip "Beklemede" durumunda randevu oluşturur.</summary>
    Task<IslemSonucu<Randevu>> OlusturAsync(int hastaId, int doktorId, DateTime baslangic, int? belirtiKaydiId = null);

    /// <summary>Onay, iptal, tamamlandı, gelmedi. Sahiplik ve durum kuralları denetlenir.</summary>
    Task<IslemSonucu> DurumDegistirAsync(int randevuId, RandevuDurumu yeniDurum, Islemci kim);

    /// <summary>Arayüzde hangi butonların gösterileceği için.</summary>
    List<RandevuDurumu> IzinVerilenDurumlar(Randevu randevu, Islemci kim);

    DateTime Simdi { get; }

    TakvimAyarlari Ayarlar { get; }
}
