# Akıllı Randevu ve Poliklinik Yönlendirme Asistanı

Hastaların belirtilerini girerek doğru polikliniğe yönlendirildiği ve randevu alabildiği web uygulaması.

## Modüller

- **Randevu takvimi:** Doktor ve bölüme göre uygun saatleri gösterir, randevu alınır ve iptal edilir.
- **Belirti girişi:** Hasta şikâyetlerini seçer ya da yazar.
- **Bölüm yönlendirme:** Girilen belirtilere göre uygun poliklinik önerilir.
- **Hasta geçmişi:** Geçmiş randevular ve belirti kayıtları listelenir.
- **Bildirim:** Randevu hatırlatma ve iptal bildirimleri gönderilir.
- **Yönetici:** Bölüm, doktor, belirti ve kullanıcı yönetimi yapılır.

## Teknolojiler

| Alan | Teknoloji |
|---|---|
| Dil / Platform | C#, .NET 10 |
| Web | ASP.NET Core MVC |
| Veri erişimi | Entity Framework Core |
| Veritabanı | SQLite |
| Sürüm kontrolü | Git, GitHub |

## Proje yapısı

Katmanlı mimari kullanılır. Ayrıntılar için [docs/mimari.md](docs/mimari.md) dosyasına bak.

```
src/
├── AkilliRandevu.Web          → Sunum katmanı (MVC)
├── AkilliRandevu.Business     → İş katmanı
├── AkilliRandevu.DataAccess   → Veri katmanı (EF Core)
└── AkilliRandevu.Entities     → Varlıklar
```

Veri modeli için [docs/ER-diyagrami.md](docs/ER-diyagrami.md), veritabanı, migration, kısıtlar ve CRUD için [docs/veritabani.md](docs/veritabani.md) dosyasına bak.

## Çalıştırma

Gereksinim: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone <depo-adresi>
cd akilli_randevu-asistani
dotnet build
dotnet run --project src/AkilliRandevu.Web
```

Ardından tarayıcıda konsolda yazan adresi aç (ör. `http://localhost:5xxx`). İlk çalıştırmada SQLite veritabanı (`akilli_randevu.db`) sentetik örnek veriyle otomatik oluşturulur.

## Haftalık ilerleme

| Hafta | Konu | Durum |
|---|---|---|
| 3 | Kurulum, mimari ve veri modeli | ✅ |
| 4 | Veritabanı ve veri erişim katmanı | ✅ |
| 5–15 | … | ⏳ |
