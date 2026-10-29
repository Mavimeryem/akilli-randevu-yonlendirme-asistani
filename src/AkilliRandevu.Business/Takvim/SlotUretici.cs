namespace AkilliRandevu.Business.Takvim;

/// <summary>
/// Bir günün slotlarını çalışma ayarlarından üretir. Saf fonksiyondur: veritabanına gitmez,
/// bu yüzden kolayca test edilir. Doluluk bilgisi dışarıdan (mevcut randevulardan) verilir.
/// </summary>
public static class SlotUretici
{
    /// <summary>Günün tüm slot aralıkları (doluluktan bağımsız). Çalışılmayan günde boş liste.</summary>
    public static List<(DateTime Baslangic, DateTime Bitis)> Araliklar(DateOnly gun, TakvimAyarlari a)
    {
        var sonuc = new List<(DateTime, DateTime)>();
        bool haftaSonu = gun.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        if (haftaSonu && !a.HaftaSonuAcik)
            return sonuc;

        var sure = TimeSpan.FromMinutes(a.SlotDakika);
        var bitis = gun.ToDateTime(a.MesaiBitis);
        var ogleBas = gun.ToDateTime(a.OgleArasiBaslangic);
        var ogleBit = gun.ToDateTime(a.OgleArasiBitis);

        for (var t = gun.ToDateTime(a.MesaiBaslangic); t + sure <= bitis; t += sure)
        {
            // Öğle arasıyla çakışan slot atlanır.
            if (t < ogleBit && t + sure > ogleBas)
                continue;
            sonuc.Add((t, t + sure));
        }
        return sonuc;
    }

    /// <summary>Slotları; geçmişte kalanlar ve dolu aralıklar işaretlenmiş olarak üretir.</summary>
    public static List<Slot> Uret(
        DateOnly gun,
        TakvimAyarlari a,
        DateTime simdi,
        IEnumerable<(DateTime Baslangic, DateTime Bitis)> doluAraliklar)
    {
        var dolu = doluAraliklar.ToList();
        return Araliklar(gun, a)
            .Select(s =>
            {
                var durum = s.Baslangic <= simdi ? SlotDurumu.Gecmis
                          : dolu.Any(d => d.Baslangic < s.Bitis && d.Bitis > s.Baslangic) ? SlotDurumu.Dolu
                          : SlotDurumu.Bos;
                return new Slot(s.Baslangic, s.Bitis, durum);
            })
            .ToList();
    }
}
