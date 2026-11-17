using AkilliRandevu.Tests.Yardimcilar;
using Microsoft.EntityFrameworkCore;

namespace AkilliRandevu.Tests.Veritabani;

/// <summary>
/// Migration'lar elle yazıldığında (bu bilgisayarda dotnet-ef engelli) modelle uyumsuzluk riski vardır.
/// Bu testler şemanın migration'larla kurulabildiğini ve modelin snapshot ile aynı olduğunu doğrular.
/// </summary>
public class MigrationTestleri
{
    [Fact]
    public void Tum_migrationlar_uygulanir_ve_bekleyen_model_degisikligi_yoktur()
    {
        using var vt = new TestVeritabani();
        using var db = vt.YeniContext();

        Assert.Empty(db.Database.GetPendingMigrations());
        Assert.False(db.Database.HasPendingModelChanges(),
            "Model, son migration snapshot'ından farklı: yeni bir migration gerekiyor ya da snapshot hatalı.");
    }

    [Fact]
    public void Sentetik_veri_migrationla_yuklenir()
    {
        using var vt = new TestVeritabani();
        using var db = vt.YeniContext();

        Assert.Equal(8, db.Bolumler.Count());
        Assert.Equal(16, db.Belirtiler.Count());
        Assert.Equal(19, db.Kullanicilar.Count());
        Assert.Equal(10, db.Randevular.Count());
    }
}
