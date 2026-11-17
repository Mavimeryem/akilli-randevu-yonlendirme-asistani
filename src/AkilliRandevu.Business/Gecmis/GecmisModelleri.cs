using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Gecmis;

public enum GecmisTuru
{
    Randevu = 1,
    Belirti = 2
}

/// <summary>Kullanıcının seçtiği filtreler (hepsi isteğe bağlı).</summary>
public record GecmisFiltresi(
    DateOnly? Baslangic = null,
    DateOnly? Bitis = null,
    GecmisTuru? Tur = null,
    RandevuDurumu? Durum = null,
    int? BolumId = null,
    int Sayfa = 1);

/// <summary>Zaman çizelgesinde tek satır: bir randevu ya da bir belirti kaydı.</summary>
public record GecmisOlayi(
    GecmisTuru Tur,
    int KayitId,
    DateTime Tarih,            // poliklinik yerel saati
    string Baslik,             // "Kardiyoloji randevusu" / "Belirti girişi"
    string? Bolum,
    string? Doktor,
    RandevuDurumu? RandevuDurumu,
    AciliyetSeviyesi? Aciliyet,
    IReadOnlyList<string> Belirtiler,
    string? Aciklama,          // belirti açıklaması — yöneticiye gösterilmez
    string? DoktorNotu,        // yöneticiye gösterilmez
    string? YzOzeti,           // "YZ: Ortopedi (güven: yüksek)" — yöneticiye gösterilmez
    bool KendiRandevusu);      // doktor görüntülüyorsa: bu randevu ona mı ait (not ekleyebilir)

public record GecmisOzeti(int ToplamRandevu, int Tamamlanan, int IptalEdilen, int Gelmedi, int BelirtiKaydi, DateTime? SonZiyaret);

public record HastaGecmisi(
    int HastaId,
    string AdSoyad,
    int? Yas,                  // yöneticiye gösterilmez
    Cinsiyet? Cinsiyet,        // yöneticiye gösterilmez
    GecmisOzeti Ozet,
    IReadOnlyList<GecmisOlayi> Olaylar,
    int Sayfa,
    int ToplamSayfa,
    int ToplamOlay,
    bool TibbiIcerikGizli);
