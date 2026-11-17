using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace AkilliRandevu.Tests.Entegrasyon;

/// <summary>
/// Hafta 13 – uçtan uca: gerçek HTTP istekleriyle "kayıt → giriş → belirti → yönlendirme → randevu → doktor onayı →
/// bildirim → geçmiş" akışı. Her test kendi uygulama örneğini (ve veritabanını) kullanır.
/// </summary>
public class UctanUcaAkisTestleri
{
    /// <summary>Poliklinik saatine göre yarından sonraki ilk iş günü (randevu her zaman ileri tarihli olsun).</summary>
    private static DateOnly SonrakiIsGunu()
    {
        var istanbul = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        var g = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istanbul)).AddDays(1);
        while (g.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            g = g.AddDays(1);
        return g;
    }

    [Fact]
    public async Task Belirtiden_randevuya_ve_doktor_onayina_tam_akis()
    {
        using var f = new WebFabrikasi();
        var gun = SonrakiIsGunu();
        var saat = $"{gun:yyyy-MM-dd}T09:00";

        // 1) Hasta (hasta3 – Selin Kurt) bel ağrısı girer → Ortopedi önerilir.
        var hasta = f.Istemci();
        await hasta.GirisYapAsync("hasta3@ornek.test");
        var belirti = await hasta.FormGonderAsync("/Belirti/Giris", "/Belirti/Giris",
        [
            new("Belirtiler[0].BelirtiId", "14"), new("Belirtiler[0].Secili", "true"), new("Belirtiler[0].Siddet", "8"),
            new("Aciklama", "İki haftadır belim ağrıyor.")
        ]);
        Assert.Equal(HttpStatusCode.Redirect, belirti.StatusCode);
        var sonucAdresi = belirti.Headers.Location!.OriginalString;
        var sonuc = await (await hasta.GetAsync(sonucAdresi)).MetinAsync();
        Assert.Contains("Ortopedi", sonuc);
        Assert.Contains("tıbbi tanı değildir", sonuc);
        var kayitId = sonucAdresi.Split('/').Last();

        // 2) Sonuçtan Ortopedi (doktor8) randevusu alınır, kayıt randevuya bağlanır.
        var randevu = await hasta.FormGonderAsync("/Randevu/Al", "/Randevu/Al",
        [
            new("doktorId", "8"), new("baslangic", saat), new("bolumId", "6"), new("belirtiKaydiId", kayitId)
        ]);
        Assert.Equal(HttpStatusCode.Redirect, randevu.StatusCode);
        Assert.Contains("/Hasta/Randevularim", randevu.Headers.Location!.OriginalString);
        var randevularim = await (await hasta.GetAsync("/Hasta/Randevularim")).MetinAsync();
        Assert.Contains($"{gun:dd.MM.yyyy} 09:00", randevularim);
        Assert.Contains("talebiniz alındı", randevularim);

        // 3) Aynı slotu başka bir hasta alamaz.
        var baska = f.Istemci();
        await baska.GirisYapAsync("hasta4@ornek.test");
        await baska.FormGonderAsync("/Randevu/Al", "/Randevu/Al", [new("doktorId", "8"), new("baslangic", saat)]);
        var hataSayfasi = await (await baska.GetAsync($"/Randevu/Al?doktorId=8&gun={gun:yyyy-MM-dd}")).MetinAsync();
        Assert.Contains("Bu saat dolu", hataSayfasi);

        // 4) Doktor takviminde hasta ve belirtisi görünür; doktor onaylar.
        var doktor = f.Istemci();
        await doktor.GirisYapAsync("doktor8@ornek.test");
        var takvim = await (await doktor.GetAsync($"/Doktor/Takvim?gun={gun:yyyy-MM-dd}")).MetinAsync();
        Assert.Contains("Selin Kurt", takvim);
        Assert.Contains("Bel ağrısı (8/10)", takvim);
        var randevuId = System.Text.RegularExpressions.Regex.Match(takvim, "name=\"id\" value=\"(\\d+)\"").Groups[1].Value;
        var onay = await doktor.FormGonderAsync($"/Doktor/Takvim?gun={gun:yyyy-MM-dd}", "/Randevu/DurumDegistir",
            [new("id", randevuId), new("durum", "Onaylandi"), new("returnUrl", "/Doktor/Takvim")]);
        Assert.Equal(HttpStatusCode.Redirect, onay.StatusCode);

        // 5) Hasta bildirimi görür; geçmişinde randevu "Onaylandı".
        var bildirimler = await (await hasta.GetAsync("/Bildirim")).MetinAsync();
        Assert.Contains("Randevu onaylandı", bildirimler);
        var gecmis = await (await hasta.GetAsync("/Gecmis?tur=Randevu")).MetinAsync();
        Assert.Contains("Ortopedi randevusu", gecmis);
        Assert.Contains("Onaylandı", gecmis);

        // 6) Doktorun geçmiş görüntülemesi hastanın erişim kayıtlarına düşer.
        await doktor.GetAsync("/Gecmis?hastaId=3");
        var gecmisTekrar = await (await hasta.GetAsync("/Gecmis")).MetinAsync();
        Assert.Contains("Hasan Arslan", gecmisTekrar); // doktor8
    }

    [Fact]
    public async Task Yeni_hasta_kayit_olup_giris_yapabilir_sifre_duz_metin_saklanmaz()
    {
        using var f = new WebFabrikasi();
        var c = f.Istemci();

        var kayit = await c.FormGonderAsync("/Hesap/Kayit", "/Hesap/Kayit",
        [
            new("Ad", "Test"), new("Soyad", "Kullanici"), new("Eposta", "Yeni.Hasta@Ornek.Test"),
            new("TcKimlikNo", "99000000099"), new("DogumTarihi", "2000-01-01"), new("Cinsiyet", "Belirtilmedi"),
            new("Sifre", "Guclu1234"), new("SifreTekrar", "Guclu1234")
        ]);
        Assert.Equal(HttpStatusCode.Redirect, kayit.StatusCode);

        // E-posta büyük/küçük harf duyarsız: küçük harfle giriş yapılabilir.
        await c.GirisYapAsync("yeni.hasta@ornek.test", "Guclu1234");
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Gecmis")).StatusCode);

        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AkilliRandevu.DataAccess.Context.AppDbContext>();
        var hash = db.Kullanicilar.Single(k => k.Eposta == "yeni.hasta@ornek.test").SifreHash;
        Assert.DoesNotContain("Guclu1234", hash);
        Assert.StartsWith("PBKDF2-SHA256$600000$", hash);
    }

    [Fact]
    public async Task Bes_hatali_giristen_sonra_dogru_sifre_de_reddedilir()
    {
        using var f = new WebFabrikasi();
        var c = f.Istemci();

        for (int i = 0; i < 5; i++)
            await c.FormGonderAsync("/Hesap/Giris", "/Hesap/Giris", [new("Eposta", "hasta5@ornek.test"), new("Sifre", "yanlis123")]);
        var y = await c.FormGonderAsync("/Hesap/Giris", "/Hesap/Giris", [new("Eposta", "hasta5@ornek.test"), new("Sifre", "Demo1234")]);

        Assert.Equal(HttpStatusCode.OK, y.StatusCode); // yönlendirme yok: giriş yapılmadı
        Assert.Contains("Çok fazla başarısız deneme", await y.MetinAsync());
    }
}
