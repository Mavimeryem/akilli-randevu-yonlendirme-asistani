# Kurulum

## 1. Yerelde (.NET SDK ile)
Gereksinim: .NET 10 SDK.

```bash
git clone https://github.com/Mavimeryem/akilli-randevu-yonlendirme-asistani.git
cd akilli-randevu-yonlendirme-asistani
dotnet restore
dotnet run --project src/AkilliRandevu.Web
```

- Geliştirme ortamında demo hesapları (`Demo1234`) her zaman açıktır.
- Veritabanı `src/AkilliRandevu.Web/akilli_randevu.db` dosyasıdır. Silinirse bir sonraki açılışta sentetik veriyle yeniden oluşur.

### İsteğe bağlı: API anahtarları
Anahtarlar `appsettings.json` dosyasına **yazılmaz**. Yerelde kullanıcı gizli anahtarları kullanılır (depo dışında saklanır):

```bash
cd src/AkilliRandevu.Web
dotnet user-secrets set "Eposta:ApiAnahtari" "<brevo-anahtari>"
dotnet user-secrets set "Eposta:GondericiAdres" "<dogrulanmis-adres>"
```

Claude anahtarı ortam değişkeninden okunur:

```powershell
$env:ANTHROPIC_API_KEY = "<anahtar>"   # PowerShell, yalnızca o oturum için
```

## 2. Docker ile
```bash
docker build -t akilli-randevu .
docker run --rm -p 8080:8080 -e DemoModu=true -v akilli-veri:/app/veri akilli-randevu
```
Adres: `http://localhost:8080`. Sağlık kontrolü: `http://localhost:8080/saglik`.

İmaj: çok aşamalı (SDK ile derlenir, yalnızca ASP.NET çalışma zamanı ile çalışır), root olmayan kullanıcı, Türkiye saat dilimi verisi (`tzdata`) yüklü. Veritabanı ve oturum anahtarları `/app/veri` biriminde tutulur.

## 3. Testler
```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

## 4. Sorun giderme
| Belirti | Çözüm |
|---|---|
| Windows'ta derlenen uygulama/testler açılmıyor ("Akıllı Uygulama Denetimi bu dosyayı engelledi") | Windows'un imzasız DLL koruması. Testleri GitHub Actions'ta çalıştırın ya da Docker / GitHub Codespaces kullanın |
| Giriş yapıldıktan sonra tekrar girişe dönüyor | Tarayıcı çerezleri engelliyor olabilir; proxy arkasındaysa `ProxyArkasinda=true` verin |
| Yapay zekâ kutusu "kapalı" diyor | `ANTHROPIC_API_KEY` tanımlı değil; kural tabanlı öneri yine çalışır |
| Saatler 3 saat kaymış | Sunucuda `tzdata` yok; Docker imajında kurulu, başka ortamda `Takvim:SaatDilimi` ayarını kontrol edin |
