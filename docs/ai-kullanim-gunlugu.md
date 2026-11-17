# Yapay Zekâ Kullanım Günlüğü

Bu belge, proje boyunca yapay zekâ araçlarının hangi amaçlarla kullanıldığını ve
üretilen çıktıların nasıl değerlendirildiğini kaydeder.

---

## Hafta 3 – Kurulum, Mimari ve Veri Modeli

| Görev | Araç | Kullanım amacı | Değerlendirme |
|---|---|---|---|
| Katmanlı mimari kararı | Claude | Entities / DataAccess / Business / Web ayrımı sorgulandı | Öneri benimsendi; bağımlılık yönü elle kontrol edildi |
| ER diyagramı taslağı | Claude | Tablo ve ilişki önerileri istendi | `BelirtiBolumleri` çok-çoka tablosu eklendi, gereksiz alanlar çıkarıldı |
| SQLite seçimi | Claude | Geliştirme için SQLite vs PostgreSQL karşılaştırması | SQLite tercih edildi; üretim için not düşüldü |

---

## Hafta 4 – Veritabanı ve Veri Erişim Katmanı

| Görev | Araç | Kullanım amacı | Değerlendirme |
|---|---|---|---|
| EF Core migration akışı | Claude | İlk migration ve seed veri yapısı | Migration kodu incelendi, gereksiz cascade ayarları düzeltildi |
| SQLite transaction kilitleme | Claude | `BEGIN IMMEDIATE` neden gerekli? | Açıklama doğrulandı; slot çakışma testi elle yapıldı |
| Index önerileri | Claude | Hangi sütunlara index eklenmeli | Öneri benimsendi; `EXPLAIN QUERY PLAN` ile doğrulandı |

---

## Hafta 5 – Kullanıcı Yönetimi ve Kimlik Doğrulama

| Görev | Araç | Kullanım amacı | Değerlendirme |
|---|---|---|---|
| Cookie tabanlı kimlik doğrulama | Claude | ASP.NET Core cookie auth yapılandırması | Kod şablonu alındı, `SlidingExpiration` ve `HttpOnly` ayarları elle eklendi |
| Şifre hashleme | Claude | Parola hash yöntemleri (PBKDF2, BCrypt, Argon2) karşılaştırması | .NET'te ek paket gerektirmeyen PBKDF2-SHA256 (600.000 tekrar, rastgele tuz) seçildi – `Pbkdf2SifreHasher` |
| Rol tabanlı yetkilendirme | Claude | `[Authorize(Roles)]` kullanımı | Öneri doğruydu; IDOR koruması ayrıca elle eklendi |

---

## Hafta 6 – Randevu Takvimi ve Yönetimi

| Görev | Araç | Kullanım amacı | Değerlendirme |
|---|---|---|---|
| Slot üretici algoritması | Claude | Mesai saatlerine göre 20 dk slot listesi | Algoritma doğrulandı; öğle arası ve hafta sonu koşulları elle test edildi |
| Durum makinesi tasarımı | Claude | Randevu durumları ve geçiş kuralları | Mermaid diyagramı üretildi; "Gelmedi" durumu kendi kararımla eklendi |
| Çakışma koruması | Claude | Transaction ve kilitleme stratejisi | `BEGIN IMMEDIATE` ile 5 eşzamanlı istek testi elle yapıldı |

---

## Hafta 7 – Belirti Girişi ve Temel Yönlendirme

| Görev | Araç | Kullanım amacı | Değerlendirme |
|---|---|---|---|
| Yönlendirme algoritması | Claude | Ağırlık × şiddet puanlama formülü | Formula benimsendi; eşik değerleri (%40, 10 puan farkı) hekim literatürü gözetilerek ayarlandı |
| Acil kural listesi | Claude | Hangi belirti kombinasyonları acile işaret eder | Öneri başlangıç noktası olarak alındı; "inme şüphesi" kuralı (uyuşma + bulanık görme) elle eklendi |
| Belirsizlik tespiti | Claude | Belirsiz sonuç koşulları | Dahiliye yönlendirmesi fikri benimsendi; eşik değerleri tartışıldı ve %40 seçildi |

---

## Hafta 8 – Ara Sürüm Entegrasyonu

| Görev | Araç | Kullanım amacı | Değerlendirme |
|---|---|---|---|
| Uçtan uca akış incelemesi | Claude | Mevcut controller ve view bağlantılarının eksik var mı kontrolü | Eksik bağlantı bulunmadı; entegrasyon tamam |
| Demo senaryosu | Claude | Adım adım demo taslağı | Taslak gözden geçirildi, acil durum senaryosu eklendi |
| Bilinen hata taraması | Claude | Kod gözden geçirme | `program.cs` kök dosyası tespit edildi ve silindi |
| Semantic versioning | Claude | v0.8.0 etiketinin anlamı | Semver belgesi okundu, etiket benimsendi |

---

## Hafta 9 – Harici API Entegrasyonu (E-posta Bildirimi)

| Görev | Araç | Kullanım amacı | Değerlendirme |
|---|---|---|---|
| Brevo API dokümantasyonu | Claude (Claude Code) | `POST /v3/smtp/email` istek/yanıt biçimi, `api-key` başlığı, hata kodları | İstek ve yanıt örnekleri testlere çevrildi (`BrevoEpostaGondericiTestleri`) |
| Hata ve zaman aşımı yönetimi | Claude (Claude Code) | 429/`Retry-After`, 5xx üstel bekleme, kalıcı/geçici hata ayrımı | 7 birim testiyle (sahte HTTP sunucusu) GitHub Actions'ta doğrulandı |
| Giden kutusu (outbox) ve hatırlatma zamanlayıcısı | Claude (Claude Code) | E-postanın transaction dışında gönderilmesi, aynı randevuya tek hatırlatma | Gerçek SQLite veritabanıyla testler; migration elle yazıldığı için model uyumu testi eklendi |

---

## Hafta 10 – Yapay Zekâ Bileşeni (Belirtiden Bölüm Yönlendirme)

Bu hafta yapay zekâ hem geliştirme aracı hem de **ürünün parçası** olarak kullanıldı.

| Görev | Araç | Kullanım amacı | Değerlendirme |
|---|---|---|---|
| Claude C# SDK kullanımı | Claude (Claude Code) | Yapılandırılmış çıktı (`output_config.format`), hata sınıfları, yedek model ayarı | SDK sürüm 13.9.0'daki tür adları DLL'den doğrulandı; derleyiciyle kontrol edildi |
| İstem tasarımı (v1) | Claude (Claude Code) | Rol, sınırlar (tanı yok), güven tanımı, acil işaretleri, istem enjeksiyonuna karşı etiketleme | İstemin tamamı `docs/yapay-zeka-bileseni.md` içinde belgelendi; içerik testleri var |
| Çıktı doğrulama ve hata yönetimi | Claude (Claude Code) | Listede olmayan bölüm, boş/kesik yanıt, ret, servis hatası | 32 birim/entegrasyon testi (sahte model istemcisi) GitHub Actions'ta çalışıyor |

**Ürün içindeki yapay zekâ çağrılarının günlüğü:** Her çağrının modeli, istem sürümü, süresi, sonucu ve hatası `YzDegerlendirmeleri` tablosunda tutuluyor.

---

## Genel Değerlendirme

Yapay zekâ araçları bu projede şu amaçlarla kullanıldı:

- **Şablonlama:** Tekrarlayan kod yapıları (controller, repository, view) için başlangıç taslakları.
- **Mimari tartışma:** Tasarım kararlarını tartışmak ve alternatif yaklaşımları anlamak.
- **Belgeleme:** Modül belgelerinin taslaklandırılması.
- **Hata ayıklama:** Hata mesajlarını yorumlamak ve çözüm önerileri almak.

Her değişiklik elle incelendi ve projeye uyarlandı. Üretilen kod doğrudan kopyalanmadı,
gerektiğinde değiştirildi veya reddedildi.
