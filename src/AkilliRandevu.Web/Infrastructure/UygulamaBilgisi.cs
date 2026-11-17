namespace AkilliRandevu.Web.Infrastructure;

/// <summary>Görünümlerin bilmesi gereken uygulama düzeyindeki ayarlar.</summary>
/// <param name="DemoModu">Sentetik hesaplar açık mı (geliştirme ya da DemoModu=true).</param>
public record UygulamaBilgisi(bool DemoModu);
