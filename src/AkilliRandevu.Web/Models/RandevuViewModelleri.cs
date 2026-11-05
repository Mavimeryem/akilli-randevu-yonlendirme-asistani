using AkilliRandevu.Business.Takvim;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Web.Models;

/// <summary>Listedeki bir randevu ve giriş yapan kişinin onu hangi durumlara geçirebileceği.</summary>
public record RandevuSatiri(Randevu Randevu, List<RandevuDurumu> Izinler);

public class RandevuAlViewModel
{
    public List<Bolum> Bolumler { get; set; } = [];
    public int? BolumId { get; set; }
    public int? DoktorId { get; set; }

    /// <summary>Belirti yönlendirmesinden gelindiyse randevu bu kayda bağlanır.</summary>
    public int? BelirtiKaydiId { get; set; }

    public DateOnly Gun { get; set; }
    public DateOnly EnErkenGun { get; set; }
    public DateOnly EnGecGun { get; set; }
    public List<Slot> Slotlar { get; set; } = [];

    public Bolum? SeciliBolum => Bolumler.FirstOrDefault(b => b.Id == BolumId);
    public Doktor? SeciliDoktor => SeciliBolum?.Doktorlar.FirstOrDefault(d => d.Id == DoktorId);
}

/// <summary>Doktor takviminde bir satır: slot ve (varsa) o slottaki randevu.</summary>
public record TakvimSatiri(DateTime Baslangic, DateTime Bitis, SlotDurumu SlotDurumu, RandevuSatiri? Randevu);

public class DoktorTakvimViewModel
{
    public DateOnly Gun { get; set; }
    public List<TakvimSatiri> Satirlar { get; set; } = [];
}

public class YonetimRandevularViewModel
{
    public DateOnly Gun { get; set; }
    public int? BolumId { get; set; }
    public List<Bolum> Bolumler { get; set; } = [];
    public List<RandevuSatiri> Randevular { get; set; } = [];
}
