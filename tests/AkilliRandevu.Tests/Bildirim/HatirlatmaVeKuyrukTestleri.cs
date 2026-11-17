using AkilliRandevu.Business.Bildirimler;
using AkilliRandevu.DataAccess.Concrete;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Enums;
using AkilliRandevu.Tests.Yardimcilar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Tests.Bildirim;

/// <summary>Hafta 9: hatırlatma zamanlaması ve e-posta giden kutusu (gerçek veritabanı + sahte e-posta servisi).</summary>
public class HatirlatmaVeKuyrukTestleri : IDisposable
{
    private readonly TestVeritabani _vt = new();

    public void Dispose() => _vt.Dispose();

    private sealed class SahteGonderici : IEpostaGonderici
    {
        public Queue<EpostaSonucu> Sonuclar { get; } = new();
        public List<EpostaMesaji> Gonderilenler { get; } = new();

        public Task<EpostaSonucu> GonderAsync(EpostaMesaji mesaj, CancellationToken ct = default)
        {
            Gonderilenler.Add(mesaj);
            return Task.FromResult(Sonuclar.Count > 0 ? Sonuclar.Dequeue() : new EpostaSonucu(EpostaSonucTuru.Gonderildi, 1, "id"));
        }
    }

    private static EpostaAyarlari Yapilandirilmis(string? yonlendirme = null) =>
        new() { ApiAnahtari = "x", GondericiAdres = "noreply@ornek.com", YonlendirmeAdresi = yonlendirme };

    private static BildirimService BildirimServisi(AppDbContext db, EpostaAyarlari eposta) =>
        new(new EfBildirimRepository(db), new EfKullaniciRepository(db), Options.Create(eposta));

    private static HatirlatmaServisi HatirlatmaServisi(AppDbContext db, SabitSaat saat, EpostaAyarlari eposta) =>
        new(new EfRandevuRepository(db), BildirimServisi(db, eposta), new EfIslemYoneticisi(db), saat,
            Options.Create(new BildirimAyarlari { HatirlatmaSaatOnce = 24 }));

    [Fact]
    public async Task Yirmi_dort_saat_icindeki_aktif_randevulara_bir_kez_hatirlatma_olusturulur()
    {
        // Örnek veride 20.10.2026: 09:00 (beklemede, hasta1) ve 13:00 (onaylı, hasta5) randevuları var.
        var saat = new SabitSaat(new DateTime(2026, 10, 19, 14, 0, 0));

        await using (var db = _vt.YeniContext())
            Assert.Equal(2, await HatirlatmaServisi(db, saat, Yapilandirilmis()).HatirlatmalariOlusturAsync());

        // Servis tekrar çalışsa da aynı randevulara ikinci hatırlatma gitmez.
        await using (var db = _vt.YeniContext())
            Assert.Equal(0, await HatirlatmaServisi(db, saat, Yapilandirilmis()).HatirlatmalariOlusturAsync());

        await using (var db = _vt.YeniContext())
        {
            var hatirlatmalar = await db.Bildirimler.Include(b => b.EpostaGonderimleri)
                .Where(b => b.Baslik == "Randevu hatırlatma" && b.Id > 6).ToListAsync();
            Assert.Equal(2, hatirlatmalar.Count);
            Assert.All(hatirlatmalar, b => Assert.Equal(GonderimDurumu.Bekliyor, Assert.Single(b.EpostaGonderimleri).Durum));
            Assert.Equal(2, await db.Randevular.CountAsync(r => r.HatirlatmaTarihi != null));
        }
    }

    [Fact]
    public async Task Iptal_edilmis_ve_24_saatten_uzak_randevular_hatirlatilmaz()
    {
        // 12.10.2026 10:00 → 13.10 09:40 randevusu iptal edilmiş; 24 saat içinde başka randevu yok.
        var saat = new SabitSaat(new DateTime(2026, 10, 12, 10, 0, 0));

        await using var db = _vt.YeniContext();
        Assert.Equal(0, await HatirlatmaServisi(db, saat, Yapilandirilmis()).HatirlatmalariOlusturAsync());
    }

    [Fact]
    public async Task Eposta_servisi_ayarli_degilse_gonderim_kaydi_Atlandi_olur_ama_bildirim_olusur()
    {
        await using var db = _vt.YeniContext();
        await BildirimServisi(db, new EpostaAyarlari()).OlusturAsync(12, "Deneme", "Mesaj");

        var b = await db.Bildirimler.Include(x => x.EpostaGonderimleri).OrderBy(x => x.Id).LastAsync();
        Assert.Equal("Deneme", b.Baslik);
        Assert.Equal(GonderimDurumu.Atlandi, Assert.Single(b.EpostaGonderimleri).Durum);
    }

    private async Task<(EpostaKuyrugu Kuyruk, SahteGonderici Gonderici, AppDbContext Db)> KuyrukKur(
        EpostaAyarlari eposta, int enFazlaKuyrukDenemesi = 5)
    {
        var db = _vt.YeniContext();
        await BildirimServisi(db, eposta).OlusturAsync(12, "Randevu onaylandı", "20.10.2026 09:00 randevunuz onaylandı.");
        var gonderici = new SahteGonderici();
        var kuyruk = new EpostaKuyrugu(new EfBildirimRepository(db), gonderici, new SabitSaat(DateTime.Now),
            Options.Create(eposta), Options.Create(new BildirimAyarlari { EnFazlaKuyrukDenemesi = enFazlaKuyrukDenemesi }));
        return (kuyruk, gonderici, db);
    }

    [Fact]
    public async Task Kuyruktaki_eposta_gonderilince_durum_ve_mesaj_kimligi_kaydedilir()
    {
        var (kuyruk, gonderici, db) = await KuyrukKur(Yapilandirilmis());
        await using var _ = db;

        var sonuc = await kuyruk.BekleyenleriGonderAsync();

        Assert.Equal(1, sonuc.Gonderilen);
        Assert.Equal("hasta1@ornek.test", Assert.Single(gonderici.Gonderilenler).Alici);
        var e = await db.EpostaGonderimleri.AsNoTracking().SingleAsync(x => x.Durum == GonderimDurumu.Gonderildi);
        Assert.Equal("id", e.DisMesajId);
        Assert.NotNull(e.GonderimTarihi);
    }

    [Fact]
    public async Task Gecici_hatada_kuyrukta_kalir_deneme_siniri_dolunca_basarisiz_olur()
    {
        var (kuyruk, gonderici, db) = await KuyrukKur(Yapilandirilmis(), enFazlaKuyrukDenemesi: 4);
        await using var _ = db;
        gonderici.Sonuclar.Enqueue(new EpostaSonucu(EpostaSonucTuru.GeciciHata, 3, Hata: "HTTP 503"));
        gonderici.Sonuclar.Enqueue(new EpostaSonucu(EpostaSonucTuru.GeciciHata, 3, Hata: "HTTP 503"));

        var tur1 = await kuyruk.BekleyenleriGonderAsync();
        Assert.Equal(1, tur1.Bekleyen); // 3 deneme < 4 → tekrar denenecek

        var tur2 = await kuyruk.BekleyenleriGonderAsync();
        Assert.Equal(1, tur2.Basarisiz); // 6 deneme ≥ 4 → vazgeçildi

        var e = await db.EpostaGonderimleri.AsNoTracking().OrderBy(x => x.Id).LastAsync();
        Assert.Equal(GonderimDurumu.Basarisiz, e.Durum);
        Assert.Equal(6, e.DenemeSayisi);
        Assert.Equal("HTTP 503", e.SonHata);
    }

    [Fact]
    public async Task Kalici_hatada_tekrar_denenmez()
    {
        var (kuyruk, gonderici, db) = await KuyrukKur(Yapilandirilmis());
        await using var _ = db;
        gonderici.Sonuclar.Enqueue(new EpostaSonucu(EpostaSonucTuru.KaliciHata, 1, Hata: "HTTP 400: invalid_parameter"));

        await kuyruk.BekleyenleriGonderAsync();
        var ikinciTur = await kuyruk.BekleyenleriGonderAsync();

        Assert.Single(gonderici.Gonderilenler);
        Assert.Equal(0, ikinciTur.Gonderilen + ikinciTur.Bekleyen + ikinciTur.Basarisiz);
    }

    [Fact]
    public async Task Demo_yonlendirmesi_aciksa_eposta_yonlendirme_adresine_gider_asil_alici_metne_yazilir()
    {
        var (kuyruk, gonderici, db) = await KuyrukKur(Yapilandirilmis(yonlendirme: "demo@ornek.com"));
        await using var _ = db;

        await kuyruk.BekleyenleriGonderAsync();

        var m = Assert.Single(gonderici.Gonderilenler);
        Assert.Equal("demo@ornek.com", m.Alici);
        Assert.Contains("hasta1@ornek.test", m.Metin);
    }
}
