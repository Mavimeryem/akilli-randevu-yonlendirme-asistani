using AkilliRandevu.Business.Concrete;
using AkilliRandevu.Business.YapayZeka;
using AkilliRandevu.Business.Yonlendirme;
using AkilliRandevu.DataAccess.Concrete;
using AkilliRandevu.DataAccess.Context;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;
using AkilliRandevu.Tests.Yardimcilar;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.Tests.YapayZeka;

/// <summary>Hafta 10 entegrasyon: belirti girişi + yapay zekâ değerlendirmesinin kaydı ve kural motoruyla ilişkisi.</summary>
public class BelirtiServisiYapayZekaTestleri : IDisposable
{
    private readonly TestVeritabani _vt = new();

    public void Dispose() => _vt.Dispose();

    private static BelirtiService Servis(AppDbContext db, IYapayZekaIstemcisi istemci) =>
        new(new EfRepository<Belirti>(db), new EfBelirtiKaydiRepository(db), new EfBolumRepository(db),
            new EfRepository<Hasta>(db), new EfRepository<YzDegerlendirmesi>(db),
            new BolumOneriAsistani(istemci), TimeProvider.System);

    private static BolumOneriAsistaniTestleri.SahteIstemci Istemci(string json) =>
        new() { Yanit = () => new YzHamYanit(json, false, "sahte-model", "end_turn") };

    [Fact]
    public async Task Onay_verilirse_yapay_zeka_degerlendirmesi_kaydedilir()
    {
        await using var db = _vt.YeniContext();
        var istemci = Istemci("""{"bolum_id":6,"gerekce":"Bel ağrısı ortopediye işaret ediyor.","guven":"yuksek","acil_belirti_var":false,"acil_gerekce":""}""");

        var sonuc = await Servis(db, istemci).DegerlendirAsync(4, [new SecilenBelirti(14, 8)], "İki haftadır belim ağrıyor.", yapayZekaOnayi: true);

        Assert.True(sonuc.Basarili);
        var yz = await db.YzDegerlendirmeleri.AsNoTracking().SingleAsync(x => x.BelirtiKaydiId == sonuc.Veri);
        Assert.Equal(YzDurumu.Basarili, yz.Durum);
        Assert.Equal(6, yz.OnerilenBolumId);
        Assert.Equal(YzGuven.Yuksek, yz.Guven);
        Assert.Equal("v1", yz.IstemSurumu);
    }

    [Fact]
    public async Task Onay_yoksa_yapay_zeka_cagrilmaz()
    {
        await using var db = _vt.YeniContext();
        var istemci = Istemci("{}");

        var sonuc = await Servis(db, istemci).DegerlendirAsync(4, [new SecilenBelirti(14, 8)], "İki haftadır belim ağrıyor.", yapayZekaOnayi: false);

        Assert.True(sonuc.Basarili);
        Assert.Equal(0, istemci.CagriSayisi);
        Assert.False(await db.YzDegerlendirmeleri.AnyAsync());
    }

    [Fact]
    public async Task Onay_varken_aciklama_bossa_hata_verilir()
    {
        await using var db = _vt.YeniContext();

        var sonuc = await Servis(db, Istemci("{}")).DegerlendirAsync(4, [new SecilenBelirti(14, 8)], "", yapayZekaOnayi: true);

        Assert.False(sonuc.Basarili);
    }

    [Fact]
    public async Task Yapay_zeka_acil_isareti_aciliyeti_yukseltir_ama_dusuremez()
    {
        await using var db = _vt.YeniContext();
        var istemci = Istemci("""{"bolum_id":3,"gerekce":"Baş ağrısı nörolojiye işaret ediyor.","guven":"orta","acil_belirti_var":true,"acil_gerekce":"Ani başlayan en şiddetli baş ağrısı."}""");

        // Kurallara göre: baş ağrısı 5 → Düşük. Yapay zekâ acil işareti → en az Yüksek.
        var sonuc = await Servis(db, istemci).DegerlendirAsync(4, [new SecilenBelirti(1, 5)], "Hayatımın en kötü baş ağrısı aniden başladı.", yapayZekaOnayi: true);

        var kayit = await db.BelirtiKayitlari.AsNoTracking().SingleAsync(k => k.Id == sonuc.Veri);
        Assert.Equal(AciliyetSeviyesi.Yuksek, kayit.Aciliyet);
    }

    [Fact]
    public async Task Kural_acil_dediginde_yapay_zeka_acil_degil_dese_de_acil_kalir()
    {
        await using var db = _vt.YeniContext();
        var istemci = Istemci("""{"bolum_id":2,"gerekce":"Göğüs ağrısı kardiyolojiye işaret ediyor.","guven":"orta","acil_belirti_var":false,"acil_gerekce":""}""");

        // Göğüs ağrısı 8 → kurallara göre Acil.
        var sonuc = await Servis(db, istemci).DegerlendirAsync(4, [new SecilenBelirti(3, 8)], "Göğsümde baskı hissi var.", yapayZekaOnayi: true);

        var kayit = await db.BelirtiKayitlari.AsNoTracking().SingleAsync(k => k.Id == sonuc.Veri);
        Assert.Equal(AciliyetSeviyesi.Acil, kayit.Aciliyet);
    }

    [Fact]
    public async Task Yapay_zeka_hatasinda_kayit_yine_olusur_ve_hata_kaydedilir()
    {
        await using var db = _vt.YeniContext();
        var istemci = new BolumOneriAsistaniTestleri.SahteIstemci { Yanit = () => throw new YzIstemciHatasi("Zaman aşımı.") };

        var sonuc = await Servis(db, istemci).DegerlendirAsync(4, [new SecilenBelirti(14, 8)], "İki haftadır belim ağrıyor.", yapayZekaOnayi: true);

        Assert.True(sonuc.Basarili);
        var kayit = await db.BelirtiKayitlari.AsNoTracking().SingleAsync(k => k.Id == sonuc.Veri);
        Assert.Equal(6, kayit.OnerilenBolumId); // kural tabanlı öneri geçerli
        var yz = await db.YzDegerlendirmeleri.AsNoTracking().SingleAsync(x => x.BelirtiKaydiId == sonuc.Veri);
        Assert.Equal(YzDurumu.Hatali, yz.Durum);
        Assert.Equal("Zaman aşımı.", yz.Hata);
    }
}
