using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.DataAccess.Abstract;

public record DoktorHastasi(int HastaId, string Ad, string Soyad, DateTime SonRandevu, int RandevuSayisi);

/// <summary>Hasta geçmişi modülünün sorguları. Filtreler veritabanında (SQL WHERE) uygulanır.</summary>
public interface IHastaGecmisiRepository
{
    Task<Hasta?> GetHastaAsync(int hastaId);

    /// <summary>Doktorun bu hastayla en az bir randevusu (tedavi ilişkisi) var mı?</summary>
    Task<bool> DoktorHastaIliskisiVarMiAsync(int doktorId, int hastaId);

    /// <param name="baslangic">Dahil, poliklinik yerel saati.</param>
    /// <param name="bitis">Hariç, poliklinik yerel saati.</param>
    Task<List<Randevu>> GetRandevularAsync(int hastaId, DateTime? baslangic, DateTime? bitis, RandevuDurumu? durum, int? bolumId);

    /// <param name="baslangicUtc">Dahil, UTC.</param>
    /// <param name="bitisUtc">Hariç, UTC.</param>
    Task<List<BelirtiKaydi>> GetBelirtiKayitlariAsync(int hastaId, DateTime? baslangicUtc, DateTime? bitisUtc, int? bolumId);

    Task<List<DoktorHastasi>> GetDoktorHastalariAsync(int doktorId);

    Task ErisimKaydetAsync(ErisimKaydi kayit);

    /// <summary>hastaId verilirse o hastanın, verilmezse tüm hastaların erişim izleri (en yeniden).</summary>
    Task<List<ErisimKaydi>> GetErisimKayitlariAsync(int? hastaId, int adet);
}
