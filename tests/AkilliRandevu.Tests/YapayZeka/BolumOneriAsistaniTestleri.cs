using System.Text.Json;
using AkilliRandevu.Business.YapayZeka;
using AkilliRandevu.Entities.Concrete;
using AkilliRandevu.Entities.Enums;

namespace AkilliRandevu.Tests.YapayZeka;

/// <summary>Hafta 10: istem oluşturma, model yanıtının işlenmesi ve hata durumları (sahte model istemcisiyle).</summary>
public class BolumOneriAsistaniTestleri
{
    /// <summary>Gerçek API yerine kullanılan sahte istemci: gönderileni kaydeder, ayarlanan yanıtı döner.</summary>
    public sealed class SahteIstemci : IYapayZekaIstemcisi
    {
        public Func<YzHamYanit> Yanit { get; set; } = () => new YzHamYanit(null, false, "sahte-model", "end_turn");
        public string? SonSistem { get; private set; }
        public string? SonKullanici { get; private set; }
        public IReadOnlyDictionary<string, JsonElement>? SonSema { get; private set; }
        public int CagriSayisi { get; private set; }

        public bool Yapilandirildi { get; set; } = true;
        public string Model => "sahte-model";

        public Task<YzHamYanit> TamamlaAsync(string sistem, string kullanici, IReadOnlyDictionary<string, JsonElement> sema, CancellationToken ct)
        {
            CagriSayisi++;
            SonSistem = sistem;
            SonKullanici = kullanici;
            SonSema = sema;
            return Task.FromResult(Yanit());
        }
    }

    private static readonly List<Bolum> Bolumler =
    [
        new() { Id = 1, Ad = "Dahiliye" },
        new() { Id = 6, Ad = "Ortopedi" }
    ];

    private static readonly List<YzBelirtiGirdisi> Belirtiler = [new("Bel ağrısı", 8)];

    [Fact]
    public async Task Gecerli_yanit_basarili_sonuc_verir()
    {
        var istemci = new SahteIstemci
        {
            Yanit = () => new YzHamYanit(
                """{"bolum_id":6,"gerekce":"Bel ağrısı ortopediye işaret ediyor.","guven":"yuksek","acil_belirti_var":false,"acil_gerekce":""}""",
                false, "sahte-model", "end_turn")
        };

        var sonuc = await new BolumOneriAsistani(istemci).DegerlendirAsync(Belirtiler, "Belim çok ağrıyor", Bolumler);

        Assert.Equal(YzDurumu.Basarili, sonuc.Durum);
        Assert.Equal(6, sonuc.Oneri!.BolumId);
        Assert.Equal(BolumOneriAsistani.IstemSurumu, sonuc.IstemSurumu);
    }

    [Fact]
    public async Task Istem_bolum_listesini_belirtileri_ve_maskelenmis_metni_icerir()
    {
        var istemci = new SahteIstemci();

        await new BolumOneriAsistani(istemci).DegerlendirAsync(Belirtiler, "Belim ağrıyor, TC 12345678901", Bolumler);

        Assert.Contains("6: Ortopedi", istemci.SonSistem);
        Assert.Contains("Tanı koyma", istemci.SonSistem);
        Assert.Contains("Bel ağrısı: 8", istemci.SonKullanici);
        Assert.Contains("<hasta_metni>", istemci.SonKullanici);
        Assert.DoesNotContain("12345678901", istemci.SonKullanici);

        // Şema: bolum_id yalnızca listedeki kimliklerden biri olabilir.
        var enumlar = istemci.SonSema!["properties"].GetProperty("bolum_id").GetProperty("enum")
            .EnumerateArray().Select(e => e.GetInt32()).ToList();
        Assert.Equal([1, 6], enumlar);
    }

    [Fact]
    public async Task Listede_olmayan_bolum_gecersiz_sayilir()
    {
        var istemci = new SahteIstemci
        {
            Yanit = () => new YzHamYanit(
                """{"bolum_id":42,"gerekce":"Uydurma bir bölüm önerisi.","guven":"yuksek","acil_belirti_var":false,"acil_gerekce":""}""",
                false, "sahte-model", "end_turn")
        };

        var sonuc = await new BolumOneriAsistani(istemci).DegerlendirAsync(Belirtiler, "Belim ağrıyor", Bolumler);

        Assert.Equal(YzDurumu.Gecersiz, sonuc.Durum);
        Assert.Null(sonuc.Oneri);
    }

    [Fact]
    public async Task Bos_yanit_gecersiz_sayilir()
    {
        var istemci = new SahteIstemci { Yanit = () => new YzHamYanit("", false, "sahte-model", "end_turn") };

        var sonuc = await new BolumOneriAsistani(istemci).DegerlendirAsync(Belirtiler, "Belim ağrıyor", Bolumler);

        Assert.Equal(YzDurumu.Gecersiz, sonuc.Durum);
    }

    [Fact]
    public async Task Kesilmis_yanit_gecersiz_sayilir()
    {
        var istemci = new SahteIstemci { Yanit = () => new YzHamYanit("""{"bolum_id":6,"gere""", false, "sahte-model", "max_tokens") };

        var sonuc = await new BolumOneriAsistani(istemci).DegerlendirAsync(Belirtiler, "Belim ağrıyor", Bolumler);

        Assert.Equal(YzDurumu.Gecersiz, sonuc.Durum);
        Assert.Contains("kesildi", sonuc.Hata);
    }

    [Fact]
    public async Task Guvenlik_reddi_kaydedilir()
    {
        var istemci = new SahteIstemci { Yanit = () => new YzHamYanit(null, true, "sahte-model", "refusal") };

        var sonuc = await new BolumOneriAsistani(istemci).DegerlendirAsync(Belirtiler, "Belim ağrıyor", Bolumler);

        Assert.Equal(YzDurumu.Reddedildi, sonuc.Durum);
    }

    [Fact]
    public async Task Servis_hatasi_istisna_firlatmaz_hatali_sonuc_doner()
    {
        var istemci = new SahteIstemci { Yanit = () => throw new YzIstemciHatasi("Yapay zekâ hız sınırına takıldı (429).") };

        var sonuc = await new BolumOneriAsistani(istemci).DegerlendirAsync(Belirtiler, "Belim ağrıyor", Bolumler);

        Assert.Equal(YzDurumu.Hatali, sonuc.Durum);
        Assert.Contains("429", sonuc.Hata);
    }
}
