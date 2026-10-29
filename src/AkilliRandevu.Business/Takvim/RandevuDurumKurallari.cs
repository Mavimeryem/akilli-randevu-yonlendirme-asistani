using AkilliRandevu.Business.Dtos;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Business.Takvim;

/// <summary>
/// Randevu durum makinesi: hangi durumdan hangisine, kim, ne zaman geçebilir.
///
///   Beklemede ──onay──▶ Onaylandı ──(başladıktan sonra)──▶ Tamamlandı / Gelmedi
///       │                   │
///       └──────iptal────────┴──▶ İptal edildi
///
/// Tamamlandı, Gelmedi ve İptal edildi son durumlardır; bunlardan çıkış yoktur.
/// </summary>
public static class RandevuDurumKurallari
{
    private static readonly Dictionary<RandevuDurumu, RandevuDurumu[]> Gecisler = new()
    {
        [RandevuDurumu.Beklemede] = [RandevuDurumu.Onaylandi, RandevuDurumu.IptalEdildi, RandevuDurumu.Tamamlandi, RandevuDurumu.Gelmedi],
        [RandevuDurumu.Onaylandi] = [RandevuDurumu.IptalEdildi, RandevuDurumu.Tamamlandi, RandevuDurumu.Gelmedi],
        [RandevuDurumu.Tamamlandi] = [],
        [RandevuDurumu.Gelmedi] = [],
        [RandevuDurumu.IptalEdildi] = []
    };

    /// <summary>Bu kişi bu randevuyu şu an hangi durumlara geçirebilir? (Arayüz butonları da bunu kullanır.)</summary>
    public static List<RandevuDurumu> IzinVerilenler(Randevu r, Islemci kim, DateTime simdi, TakvimAyarlari ayar) =>
        Gecisler[r.Durum].Where(hedef => Denetle(r, hedef, kim, simdi, ayar) is null).ToList();

    /// <summary>Geçiş geçerliyse null, değilse kullanıcıya gösterilecek hata mesajı.</summary>
    public static string? Denetle(Randevu r, RandevuDurumu hedef, Islemci kim, DateTime simdi, TakvimAyarlari ayar)
    {
        // 1) Sahiplik: hasta yalnızca kendi, doktor yalnızca kendi randevusuna dokunabilir.
        bool sahibi = kim.Rol switch
        {
            Rol.Hasta => kim.HastaId == r.HastaId,
            Rol.Doktor => kim.DoktorId == r.DoktorId,
            Rol.Yonetici => true,
            _ => false
        };
        if (!sahibi)
            return "Bu randevu üzerinde işlem yetkiniz yok.";

        // 2) Durum makinesi: bu geçiş hiç tanımlı mı?
        if (!Gecisler[r.Durum].Contains(hedef))
            return "Randevu bu durumdayken bu işlem yapılamaz.";

        bool basladi = r.BaslangicZamani <= simdi;

        // 3) Rol ve zaman kuralları.
        return hedef switch
        {
            RandevuDurumu.IptalEdildi when basladi =>
                "Başlamış veya geçmiş bir randevu iptal edilemez.",
            RandevuDurumu.IptalEdildi when kim.Rol == Rol.Hasta
                                       && r.BaslangicZamani - simdi < TimeSpan.FromHours(ayar.HastaIptalSonSaat) =>
                $"Randevunuzu en geç {ayar.HastaIptalSonSaat} saat öncesine kadar iptal edebilirsiniz.",

            RandevuDurumu.Onaylandi when kim.Rol == Rol.Hasta =>
                "Randevuyu yalnızca doktor veya yönetici onaylayabilir.",
            RandevuDurumu.Onaylandi when basladi =>
                "Geçmiş bir randevu onaylanamaz.",

            // Muayene sonucu yalnızca doktorun kendisi tarafından ve randevu saati geldikten sonra girilir.
            RandevuDurumu.Tamamlandi or RandevuDurumu.Gelmedi when kim.Rol != Rol.Doktor =>
                "Muayene sonucunu yalnızca doktor girebilir.",
            RandevuDurumu.Tamamlandi or RandevuDurumu.Gelmedi when !basladi =>
                "Randevu saati gelmeden sonuç girilemez.",

            _ => null
        };
    }
}
