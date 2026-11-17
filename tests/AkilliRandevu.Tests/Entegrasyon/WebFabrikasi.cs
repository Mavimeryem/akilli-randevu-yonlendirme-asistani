using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AkilliRandevu.Tests.Entegrasyon;

/// <summary>
/// Uygulamanın tamamını (Program.cs, gerçek DI, gerçek görünümler) test içinde başlatır.
/// Her fabrika kendi geçici SQLite dosyasını kullanır; arka plan servisi ve yapay zekâ kapalıdır.
/// </summary>
public sealed class WebFabrikasi : WebApplicationFactory<Program>
{
    private readonly string _dbDosyasi = Path.Combine(Path.GetTempPath(), $"akilli_randevu_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // demo şifreleri (Demo1234) ayarlansın
        builder.UseSetting("ConnectionStrings:Varsayilan", $"Data Source={_dbDosyasi}");
        builder.UseSetting("Bildirim:ArkaPlanEtkin", "false");
        builder.UseSetting("YapayZeka:Etkin", "false");
        builder.UseSetting("Eposta:ApiAnahtari", "");
    }

    /// <summary>Çerezleri tutan, yönlendirmeleri İZLEMEYEN istemci (302'leri test edebilmek için).</summary>
    public HttpClient Istemci() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { File.Delete(_dbDosyasi); } catch (IOException) { }
    }
}

public static partial class HttpYardimci
{
    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenDeseni();

    public static async Task<string> TokenAlAsync(this HttpClient c, string sayfa)
    {
        var html = await c.GetStringAsync(sayfa);
        var m = TokenDeseni().Match(html);
        Assert.True(m.Success, $"{sayfa} sayfasında anti-forgery token bulunamadı.");
        return m.Groups[1].Value;
    }

    /// <summary>Formu gerçek kullanıcı gibi gönderir: önce sayfadan token alınır.</summary>
    public static async Task<HttpResponseMessage> FormGonderAsync(
        this HttpClient c, string tokenSayfasi, string adres, IEnumerable<KeyValuePair<string, string>> alanlar)
    {
        var token = await c.TokenAlAsync(tokenSayfasi);
        var form = new FormUrlEncodedContent(alanlar.Append(new("__RequestVerificationToken", token)));
        return await c.PostAsync(adres, form);
    }

    public static async Task GirisYapAsync(this HttpClient c, string eposta, string sifre = "Demo1234")
    {
        var y = await c.FormGonderAsync("/Hesap/Giris", "/Hesap/Giris",
            [new("Eposta", eposta), new("Sifre", sifre)]);
        Assert.Equal(HttpStatusCode.Redirect, y.StatusCode);
    }

    /// <summary>Razor çıktısında Türkçe karakterler HTML varlığı olarak kodlanır; karşılaştırma için çözülür.</summary>
    public static async Task<string> MetinAsync(this HttpResponseMessage y) =>
        WebUtility.HtmlDecode(await y.Content.ReadAsStringAsync());
}
