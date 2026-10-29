namespace AkilliRandevu.Business.Takvim;

public enum SlotDurumu
{
    Bos,
    Dolu,
    Gecmis
}

/// <summary>Doktor takviminde tek bir randevu aralığı. Veritabanında tutulmaz, ayarlardan üretilir.</summary>
public record Slot(DateTime Baslangic, DateTime Bitis, SlotDurumu Durum);
