# Veritabanı ve Veri Erişim Katmanı

## Migration

Tablolar **EF Core migration** ile oluşturulur. C# sınıflarındaki değişiklik bir migration dosyasına (`Up` / `Down`) dönüşür. Bu dosya veritabanına uygulanır ve hangi migration'ın uygulandığı `__EFMigrationsHistory` tablosunda tutulur. Böylece şema da kod gibi sürüm kontrolünde olur ve her bilgisayarda aynı şekilde kurulur.

| Dosya | Açıklama |
|---|---|
| `src/AkilliRandevu.DataAccess/Migrations/*_IlkSema.cs` | Tablolar, ilişkiler, kısıtlar, indeksler ve örnek veri |
| `docs/sql/ornek-sorgular.sql` | Elle yazılmış ilişki/kısıt, CRUD ve sorgu örnekleri |

Uygulama açılırken bekleyen migration'lar otomatik uygulanır (`VeritabaniniHazirlaAsync`). Veritabanı dosyası `src/AkilliRandevu.Web/akilli_randevu.db` olarak oluşur.

Komutlar:

```bash
dotnet tool restore
dotnet ef migrations add <Ad> --project src/AkilliRandevu.DataAccess --startup-project src/AkilliRandevu.Web
dotnet ef database update     --project src/AkilliRandevu.DataAccess --startup-project src/AkilliRandevu.Web
dotnet ef migrations script   --project src/AkilliRandevu.DataAccess --startup-project src/AkilliRandevu.Web -o docs/sql/sema.sql
```

## Kısıtlar

| Kısıt | Tablo | Amaç |
|---|---|---|
| PRIMARY KEY | Hepsi | Her satırı benzersiz tanımlar |
| FOREIGN KEY | Hasta, Doktor, Randevu, BelirtiKaydi, Bildirim, ara tablolar | Referans bütünlüğü: var olmayan kayda bağlanılamaz |
| UNIQUE | `Kullanici.Eposta`, `Hasta.TcKimlikNo`, `Bolum.Ad`, `Belirti.Ad`, `Hasta.KullaniciId`, `Doktor.KullaniciId` | Tekrar eden kayıt olmaz, 1-1 ilişkiyi garanti eder |
| CHECK | `Rol IN (1,2,3)` | Geçersiz rol girilemez |
| CHECK | `length(TcKimlikNo) = 11` | TC 11 hane olmalı |
| CHECK | `BitisZamani > BaslangicZamani` | Randevu süresi negatif olamaz |
| CHECK | `Durum BETWEEN 1 AND 5` | Geçersiz randevu durumu girilemez |
| CHECK | `AgirlikPuani` ve `Siddet` 1–10 | Puanlar aralık dışına çıkamaz |
| NOT NULL / uzunluk | Ad, Soyad, Eposta vb. | Zorunlu alanlar boş geçilemez |

### Silme davranışları

| İlişki | Davranış | Neden |
|---|---|---|
| Kullanici → Hasta/Doktor/Bildirim | CASCADE | Kullanıcı silinince ona ait profil ve bildirimler de silinir |
| Bolum → Doktor | RESTRICT | İçinde doktor olan bölüm silinemez |
| Hasta/Doktor → Randevu | RESTRICT | Randevu geçmişi kaybolmasın |
| BelirtiKaydi → Randevu | SET NULL | Kayıt silinse de randevu kalır |
| Bolum → BelirtiKaydi.OnerilenBolum | SET NULL | Öneri boşaltılır, kayıt kalır |

## İndeksler

FK sütunlarına EF Core otomatik indeks ekler. Bunlara ek olarak sık yapılan sorgular için şu indeksler var:

| İndeks | Hangi sorguyu hızlandırır |
|---|---|
| `Randevular (DoktorId, BaslangicZamani)` | Doktorun günlük takvimi, çakışma kontrolü |
| `Randevular (HastaId, BaslangicZamani)` | Hasta geçmişi |
| `BelirtiKayitlari (HastaId, OlusturmaTarihi)` | Hastanın belirti geçmişi |
| `Bildirimler (KullaniciId, OkunduMu)` | Okunmamış bildirimler |
| `Kullanicilar (Eposta)` UNIQUE | Giriş yaparken e-posta ile arama |

İndeks eklenmeyen alanlar: `Aciklama`, `DoktorNotu` gibi serbest metinler. Bu alanlarla arama yapılmıyor, ayrıca indeks her INSERT/UPDATE işlemini yavaşlatır ve yer kaplar.

## Örnek (sentetik) veri

`src/AkilliRandevu.DataAccess/Context/SentetikVeri.cs` içinde tanımlıdır ve migration ile eklenir:

8 bölüm · 16 belirti · 33 belirti-bölüm eşleşmesi · 1 yönetici · 10 doktor · 8 hasta · 6 belirti kaydı · 10 randevu · 6 bildirim

**KVKK:** Gerçek kişisel veri yoktur. TC numaraları `99` ile başlayan uydurma numaralardır, e-postalar `@ornek.test` (gerçek olmayan alan adı), telefonlar `0555 000 ....` biçimindedir. Şifreler yerine sahte bir değer tutulur. Gerçek şifre hash'leme kimlik doğrulama haftasında eklenecek.

## Veri erişim kodu (CRUD)

| Dosya | İçerik |
|---|---|
| `Abstract/IRepository.cs`, `Concrete/EfRepository.cs` | Tüm tablolar için ortak Ekle / Getir / Listele / Güncelle / Sil |
| `Abstract/IRandevuRepository.cs`, `Concrete/EfRandevuRepository.cs` | Hasta geçmişi, doktorun günlük takvimi, çakışma kontrolü |
| `Abstract/IBolumRepository.cs`, `Concrete/EfBolumRepository.cs` | Doktorlarıyla bölümler, belirtilere göre bölüm puanlama |
| `DataAccessKayitlari.cs` | DbContext ve repository'lerin dependency injection kaydı |

**ORM ve saf SQL:** Projede ORM (EF Core) kullanıldı. LINQ ile yazılan sorgular SQL'e çevrilir, parametreler otomatik eklendiği için SQL injection'a karşı güvenlidir ve veritabanı değişirse kod aynı kalır. Saf SQL daha fazla kontrol ve bazı durumlarda daha iyi performans sağlar, ama daha çok kod gerektirir ve hata yapmaya daha açıktır. Aynı sorguların SQL karşılıkları `docs/sql/ornek-sorgular.sql` dosyasındadır.
