using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.DataAccess.Context;

/// <summary>
/// Migration ile veritabanına eklenen sentetik (uydurma) örnek veri.
/// KVKK: Gerçek kişisel veri yoktur. TC numaraları 99 ile başlar, e-postalar @ornek.test,
/// telefonlar 0555 000 .. biçimindedir; isimler rastgele seçilmiş yaygın isimlerdir.
/// Migration'ların her seferinde aynı çıkması için tüm değerler (tarihler dahil) sabittir.
/// </summary>
internal static class SentetikVeri
{
    private static readonly DateTime T0 = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
    private const string SahteHash = "SENTETIK-HASH";

    public static void Ekle(ModelBuilder mb)
    {
        BolumleriEkle(mb);
        BelirtileriEkle(mb);
        KullanicilariEkle(mb);
        BelirtiKayitlariniEkle(mb);
        RandevulariEkle(mb);
        BildirimleriEkle(mb);
    }

    private static void BolumleriEkle(ModelBuilder mb)
    {
        string[] adlar =
        [
            "Dahiliye", "Kardiyoloji", "Nöroloji", "Kulak Burun Boğaz",
            "Göz Hastalıkları", "Ortopedi", "Dermatoloji", "Göğüs Hastalıkları"
        ];
        mb.Entity<Bolum>().HasData(adlar.Select((ad, i) => new Bolum
        {
            Id = i + 1,
            Ad = ad,
            OlusturmaTarihi = T0
        }));
    }

    private static void BelirtileriEkle(ModelBuilder mb)
    {
        string[] adlar =
        [
            "Baş ağrısı", "Baş dönmesi", "Göğüs ağrısı", "Çarpıntı", "Nefes darlığı",
            "Öksürük", "Ateş", "Karın ağrısı", "Bulantı", "Boğaz ağrısı",
            "Kulak ağrısı", "Bulanık görme", "Eklem ağrısı", "Bel ağrısı",
            "Ciltte döküntü", "El ve ayaklarda uyuşma"
        ];
        mb.Entity<Belirti>().HasData(adlar.Select((ad, i) => new Belirti
        {
            Id = i + 1,
            Ad = ad,
            OlusturmaTarihi = T0
        }));

        // (belirtiId, bolumId, ağırlık 1-10)
        // Bölümler: 1 Dahiliye, 2 Kardiyoloji, 3 Nöroloji, 4 KBB, 5 Göz, 6 Ortopedi, 7 Dermatoloji, 8 Göğüs
        (int b, int bl, int p)[] eslesmeler =
        [
            (1, 3, 8), (1, 1, 4), (1, 5, 3),
            (2, 3, 7), (2, 4, 6), (2, 2, 4),
            (3, 2, 9), (3, 8, 6),
            (4, 2, 9), (4, 1, 4),
            (5, 8, 8), (5, 2, 7),
            (6, 8, 8), (6, 4, 4), (6, 1, 3),
            (7, 1, 7), (7, 4, 4),
            (8, 1, 9),
            (9, 1, 7), (9, 3, 3),
            (10, 4, 9), (10, 1, 4),
            (11, 4, 10),
            (12, 5, 10), (12, 3, 5),
            (13, 6, 8), (13, 1, 4),
            (14, 6, 9), (14, 3, 4),
            (15, 7, 10), (15, 1, 3),
            (16, 3, 9), (16, 6, 5)
        ];
        mb.Entity<BelirtiBolum>().HasData(eslesmeler.Select(e => new BelirtiBolum
        {
            BelirtiId = e.b,
            BolumId = e.bl,
            AgirlikPuani = e.p
        }));
    }

    private static void KullanicilariEkle(ModelBuilder mb)
    {
        var kullanicilar = new List<Kullanici>
        {
            Kullanici(1, "Sistem", "Yöneticisi", "yonetici@ornek.test", Rol.Yonetici)
        };

        // Doktorlar: kullanıcı 2-11, doktor 1-10
        (string ad, string soyad, string unvan, int bolumId)[] doktorlar =
        [
            ("Ayşe", "Kaya", "Uzm. Dr.", 1),
            ("Mehmet", "Demir", "Doç. Dr.", 1),
            ("Zeynep", "Çelik", "Prof. Dr.", 2),
            ("Ali", "Şahin", "Uzm. Dr.", 2),
            ("Elif", "Yıldız", "Uzm. Dr.", 3),
            ("Mustafa", "Aydın", "Op. Dr.", 4),
            ("Fatma", "Öztürk", "Op. Dr.", 5),
            ("Hasan", "Arslan", "Doç. Dr.", 6),
            ("Merve", "Doğan", "Uzm. Dr.", 7),
            ("Emre", "Kılıç", "Uzm. Dr.", 8)
        ];
        for (int i = 0; i < doktorlar.Length; i++)
        {
            var d = doktorlar[i];
            kullanicilar.Add(Kullanici(i + 2, d.ad, d.soyad, $"doktor{i + 1}@ornek.test", Rol.Doktor));
        }

        // Hastalar: kullanıcı 12-19, hasta 1-8
        (string ad, string soyad, DateOnly dogum, Cinsiyet cinsiyet)[] hastalar =
        [
            ("Deniz", "Acar", new DateOnly(1990, 3, 12), Cinsiyet.Kadin),
            ("Burak", "Koç", new DateOnly(1985, 7, 4), Cinsiyet.Erkek),
            ("Selin", "Kurt", new DateOnly(2001, 11, 23), Cinsiyet.Kadin),
            ("Can", "Özdemir", new DateOnly(1978, 1, 30), Cinsiyet.Erkek),
            ("Ece", "Polat", new DateOnly(1995, 5, 17), Cinsiyet.Kadin),
            ("Kerem", "Aslan", new DateOnly(1969, 9, 8), Cinsiyet.Erkek),
            ("Gizem", "Tekin", new DateOnly(2003, 2, 14), Cinsiyet.Kadin),
            ("Oğuz", "Yavuz", new DateOnly(1958, 12, 1), Cinsiyet.Belirtilmedi)
        ];
        for (int i = 0; i < hastalar.Length; i++)
        {
            var h = hastalar[i];
            kullanicilar.Add(Kullanici(i + 12, h.ad, h.soyad, $"hasta{i + 1}@ornek.test", Rol.Hasta));
        }

        mb.Entity<Kullanici>().HasData(kullanicilar);

        mb.Entity<Doktor>().HasData(doktorlar.Select((d, i) => new Doktor
        {
            Id = i + 1,
            KullaniciId = i + 2,
            BolumId = d.bolumId,
            Unvan = d.unvan,
            OlusturmaTarihi = T0
        }));

        mb.Entity<Hasta>().HasData(hastalar.Select((h, i) => new Hasta
        {
            Id = i + 1,
            KullaniciId = i + 12,
            TcKimlikNo = $"990000000{i + 1:D2}",
            DogumTarihi = h.dogum,
            Cinsiyet = h.cinsiyet,
            OlusturmaTarihi = T0
        }));
    }

    private static Kullanici Kullanici(int id, string ad, string soyad, string eposta, Rol rol) => new()
    {
        Id = id,
        Ad = ad,
        Soyad = soyad,
        Eposta = eposta,
        SifreHash = SahteHash,
        Telefon = $"0555 000 {id:D4}",
        Rol = rol,
        OlusturmaTarihi = T0
    };

    private static void BelirtiKayitlariniEkle(ModelBuilder mb)
    {
        mb.Entity<BelirtiKaydi>().HasData(
            new BelirtiKaydi { Id = 1, HastaId = 1, OnerilenBolumId = 3, Aciklama = "Üç gündür süren baş ağrısı ve baş dönmesi.", Aciliyet = AciliyetSeviyesi.Orta, OlusturmaTarihi = T0.AddDays(2) },
            new BelirtiKaydi { Id = 2, HastaId = 2, OnerilenBolumId = 2, Aciklama = "Merdiven çıkarken göğüs ağrısı ve çarpıntı.", Aciliyet = AciliyetSeviyesi.Yuksek, OlusturmaTarihi = T0.AddDays(3) },
            new BelirtiKaydi { Id = 3, HastaId = 3, OnerilenBolumId = 4, Aciklama = "Boğaz ve kulak ağrısı, hafif ateş.", Aciliyet = AciliyetSeviyesi.Dusuk, OlusturmaTarihi = T0.AddDays(5) },
            new BelirtiKaydi { Id = 4, HastaId = 4, OnerilenBolumId = 6, Aciklama = "Bel ağrısı bacağa vuruyor.", Aciliyet = AciliyetSeviyesi.Orta, OlusturmaTarihi = T0.AddDays(8) },
            new BelirtiKaydi { Id = 5, HastaId = 5, OnerilenBolumId = 7, Aciklama = "Kollarda kaşıntılı döküntü.", Aciliyet = AciliyetSeviyesi.Dusuk, OlusturmaTarihi = T0.AddDays(10) },
            new BelirtiKaydi { Id = 6, HastaId = 1, OnerilenBolumId = 1, Aciklama = "Karın ağrısı ve bulantı.", Aciliyet = AciliyetSeviyesi.Orta, OlusturmaTarihi = T0.AddDays(13) });

        mb.Entity<BelirtiKaydiBelirti>().HasData(
            new BelirtiKaydiBelirti { BelirtiKaydiId = 1, BelirtiId = 1, Siddet = 7 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 1, BelirtiId = 2, Siddet = 5 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 2, BelirtiId = 3, Siddet = 8 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 2, BelirtiId = 4, Siddet = 6 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 3, BelirtiId = 10, Siddet = 6 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 3, BelirtiId = 11, Siddet = 5 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 3, BelirtiId = 7, Siddet = 3 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 4, BelirtiId = 14, Siddet = 8 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 5, BelirtiId = 15, Siddet = 4 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 6, BelirtiId = 8, Siddet = 6 },
            new BelirtiKaydiBelirti { BelirtiKaydiId = 6, BelirtiId = 9, Siddet = 5 });
    }

    private static void RandevulariEkle(ModelBuilder mb)
    {
        // Yerel saat; 20 dakikalık randevular.
        static DateTime S(int gun, int saat, int dk) => new(2026, 10, gun, saat, dk, 0);

        (int id, int hasta, int doktor, int? kayit, DateTime bas, RandevuDurumu durum, string? not)[] r =
        [
            (1, 1, 5, 1, S(5, 9, 0), RandevuDurumu.Tamamlandi, "Gerilim tipi baş ağrısı. Kontrol önerildi."),
            (2, 2, 3, 2, S(6, 10, 20), RandevuDurumu.Tamamlandi, "EKG ve efor testi istendi."),
            (3, 3, 6, 3, S(8, 14, 0), RandevuDurumu.Tamamlandi, "Üst solunum yolu enfeksiyonu."),
            (4, 4, 8, 4, S(12, 11, 0), RandevuDurumu.Gelmedi, null),
            (5, 6, 1, null, S(13, 9, 40), RandevuDurumu.IptalEdildi, null),
            (6, 5, 9, 5, S(20, 13, 0), RandevuDurumu.Onaylandi, null),
            (7, 1, 1, 6, S(20, 9, 0), RandevuDurumu.Beklemede, null),
            (8, 2, 3, null, S(21, 10, 20), RandevuDurumu.Onaylandi, null),
            (9, 7, 7, null, S(22, 15, 0), RandevuDurumu.Beklemede, null),
            (10, 8, 10, null, S(23, 9, 20), RandevuDurumu.Beklemede, null)
        ];
        mb.Entity<Randevu>().HasData(r.Select(x => new Randevu
        {
            Id = x.id,
            HastaId = x.hasta,
            DoktorId = x.doktor,
            BelirtiKaydiId = x.kayit,
            BaslangicZamani = x.bas,
            BitisZamani = x.bas.AddMinutes(20),
            Durum = x.durum,
            DoktorNotu = x.not,
            OlusturmaTarihi = T0
        }));
    }

    private static void BildirimleriEkle(ModelBuilder mb)
    {
        mb.Entity<Bildirim>().HasData(
            new Bildirim { Id = 1, KullaniciId = 12, Baslik = "Randevu hatırlatma", Mesaj = "20.10.2026 09:00 Dahiliye randevunuz var.", OkunduMu = false, OlusturmaTarihi = T0.AddDays(18) },
            new Bildirim { Id = 2, KullaniciId = 16, Baslik = "Randevu onaylandı", Mesaj = "20.10.2026 13:00 Dermatoloji randevunuz onaylandı.", OkunduMu = true, OlusturmaTarihi = T0.AddDays(11) },
            new Bildirim { Id = 3, KullaniciId = 17, Baslik = "Randevu iptal edildi", Mesaj = "13.10.2026 09:40 Dahiliye randevunuz iptal edildi.", OkunduMu = true, OlusturmaTarihi = T0.AddDays(12) },
            new Bildirim { Id = 4, KullaniciId = 13, Baslik = "Randevu onaylandı", Mesaj = "21.10.2026 10:20 Kardiyoloji randevunuz onaylandı.", OkunduMu = false, OlusturmaTarihi = T0.AddDays(14) },
            new Bildirim { Id = 5, KullaniciId = 4, Baslik = "Yeni randevu", Mesaj = "21.10.2026 10:20 için yeni randevu talebi.", OkunduMu = false, OlusturmaTarihi = T0.AddDays(14) },
            new Bildirim { Id = 6, KullaniciId = 1, Baslik = "Sistem", Mesaj = "Örnek veri yüklendi.", OkunduMu = true, OlusturmaTarihi = T0 });
    }
}
