namespace AkilliRandevu.Business.Takvim;

/// <summary>
/// Poliklinik çalışma düzeni. appsettings.json içindeki "Takvim" bölümünden okunur;
/// bölüm yoksa buradaki varsayılanlar kullanılır.
/// </summary>
public class TakvimAyarlari
{
    public TimeOnly MesaiBaslangic { get; set; } = new(9, 0);
    public TimeOnly MesaiBitis { get; set; } = new(17, 0);
    public TimeOnly OgleArasiBaslangic { get; set; } = new(12, 0);
    public TimeOnly OgleArasiBitis { get; set; } = new(13, 0);

    /// <summary>Bir randevunun (slotun) süresi.</summary>
    public int SlotDakika { get; set; } = 20;

    public bool HaftaSonuAcik { get; set; }

    /// <summary>En fazla kaç gün sonrasına randevu alınabilir.</summary>
    public int EnFazlaIleriGun { get; set; } = 30;

    /// <summary>Hasta, randevusunu en geç başlangıçtan kaç saat önce iptal edebilir.</summary>
    public int HastaIptalSonSaat { get; set; } = 2;
}
