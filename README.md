# Akıllı Randevu ve Poliklinik Yönlendirme Asistanı

[![CI](https://github.com/Mavimeryem/akilli-randevu-yonlendirme-asistani/actions/workflows/ci.yml/badge.svg)](https://github.com/Mavimeryem/akilli-randevu-yonlendirme-asistani/actions/workflows/ci.yml)

Hasta belirtilerini girer, sistem uygun polikliniği önerir (kural tabanlı + isteğe bağlı yapay zekâ) ve hasta boş bir saate randevu alır. Doktor günlük takvimini ve hasta geçmişini görür; yönetici istatistikleri izler.

> **Proje:** P06 – Bilgisayar Uygulamaları I, Bingöl Üniversitesi · **Öğrenci:** Meryem MAVİ (251161042)
>
> **Canlı demo:** _Render'a dağıtıldıktan sonra adres buraya eklenecek_ (adımlar: [docs/dagitim.md](docs/dagitim.md))
>
> ⚠️ Bu bir ders projesidir. Gerçek bir sağlık kuruluşu değildir, tıbbi tavsiye vermez; bütün veriler sentetiktir.

## Özellikler

| Rol | Yapabilecekleri |
|---|---|
| **Hasta** | Kayıt ve giriş · belirti seçme, şiddet ve açıklama girme · bölüm önerisi ve aciliyet uyarısı · isteğe bağlı yapay zekâ değerlendirmesi (açık onayla) · boş saatten randevu alma ve iptal · kendi geçmişini filtreleme · bildirimler |
| **Doktor** | Günlük slot takvimi · randevu onaylama, tamamlama, gelmedi işaretleme · muayene notu · yalnızca kendi hastalarının geçmişi · sıra panosu |
| **Yönetici** | Tüm randevular ve kullanıcılar · istatistikler (yoğunluk, doluluk, iptal/gelmeme, bekleme) ve CSV dışa aktarma · e-posta gönderim kayıtları · hasta geçmişi erişim izleri |

Arka planda: randevudan 24 saat önce hatırlatma, randevu olaylarında uygulama içi bildirim ve (anahtar verilirse) e-posta.

## Teknolojiler

| Alan | Teknoloji |
|---|---|
| Platform | C#, .NET 10, ASP.NET Core MVC, Razor |
| Veri | Entity Framework Core 10, SQLite |
| Yapay zekâ | Claude API (`claude-opus-5-5`), resmi `Anthropic` C# SDK |
| E-posta | Brevo REST API |
| Test | xUnit, WebApplicationFactory, coverlet, ReportGenerator |
| CI / dağıtım | GitHub Actions, Docker, Render |

## Hızlı başlangıç

Gereksinim: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/Mavimeryem/akilli-randevu-yonlendirme-asistani.git
cd akilli-randevu-yonlendirme-asistani
dotnet run --project src/AkilliRandevu.Web
```

Konsolda yazan adresi tarayıcıda aç. İlk açılışta SQLite veritabanı sentetik verilerle oluşturulur. Docker ile çalıştırma ve ayrıntılar: [docs/kurulum.md](docs/kurulum.md).

### Demo hesapları (şifre `Demo1234`)
Yalnızca geliştirme ortamında ya da `DemoModu=true` iken açıktır.

| Rol | E-posta |
|---|---|
| Yönetici | `yonetici@ornek.test` |
| Doktor | `doktor1@ornek.test` … `doktor10@ornek.test` |
| Hasta | `hasta1@ornek.test` … `hasta8@ornek.test` |

Adım adım sunum akışı: [docs/demo-senaryosu.md](docs/demo-senaryosu.md)

## Ortam değişkenleri (özet)

| Değişken | Gerekli mi? | Açıklama |
|---|---|---|
| `ANTHROPIC_API_KEY` | Hayır | Verilmezse yapay zekâ bileşeni kapalı, kural tabanlı öneri çalışır |
| `Eposta__ApiAnahtari`, `Eposta__GondericiAdres` | Hayır | Verilmezse yalnızca uygulama içi bildirim |
| `DemoModu` | Hayır | `true`: demo hesapları açık, sayfa altında demo uyarısı |
| `IlkYonetici__Eposta`, `IlkYonetici__Sifre` | Hayır | İlk yönetici hesabını oluşturur |

Tam liste: [docs/teknik-dokumantasyon.md](docs/teknik-dokumantasyon.md#6-yapılandırma-ve-ortam-değişkenleri). **API anahtarları depoya yazılmaz**; yerelde `dotnet user-secrets`, sunucuda platformun ortam değişkenleri ekranı kullanılır.

## Testler

```bash
dotnet test
```

171 test (birim, bileşen, web ve uçtan uca). Satır kapsamı yaklaşık %89. Her push'ta GitHub Actions; derleme, migration/model uyumu, testler, kapsam raporu ve Docker imajı duman testini çalıştırır. Rapor: [docs/test-raporu.md](docs/test-raporu.md).

## Belgeler

| Belge | İçerik |
|---|---|
| [mimari.md](docs/mimari.md) | Katmanlar, modüller, klasör yapısı |
| [teknik-dokumantasyon.md](docs/teknik-dokumantasyon.md) | Modüller, HTTP uç noktaları, dış servisler, yapılandırma |
| [ER-diyagrami.md](docs/ER-diyagrami.md), [veritabani.md](docs/veritabani.md) | Veri modeli, migration'lar, kısıtlar |
| [kurulum.md](docs/kurulum.md), [dagitim.md](docs/dagitim.md) | Yerel kurulum, Docker, Render'a dağıtım |
| [guvenlik-notu.md](docs/guvenlik-notu.md) | Kimlik doğrulama, yetki, KVKK |
| [randevu-modulu.md](docs/randevu-modulu.md), [belirti-yonlendirme.md](docs/belirti-yonlendirme.md) | Randevu ve yönlendirme kuralları |
| [bildirim-entegrasyonu.md](docs/bildirim-entegrasyonu.md), [yapay-zeka-bileseni.md](docs/yapay-zeka-bileseni.md) | Dış API entegrasyonları |
| [hasta-gecmisi.md](docs/hasta-gecmisi.md), [istatistik-ve-sira-panosu.md](docs/istatistik-ve-sira-panosu.md) | Geçmiş, istatistik, sıra panosu |
| [test-raporu.md](docs/test-raporu.md), [hata-listesi.md](docs/hata-listesi.md) | Test sonuçları ve hatalar |
| [uctan-uca-dogrulama.md](docs/uctan-uca-dogrulama.md) | Gereksinim → doğrulama eşlemesi, sunum öncesi kontrol listesi |
| [final-demo-senaryosu.md](docs/final-demo-senaryosu.md), [sunum-taslagi.md](docs/sunum-taslagi.md) | Savunma demosu ve sunum anahattı |
| [CHANGELOG.md](CHANGELOG.md) | Sürüm geçmişi (`v0.8.0`, `v1.0.0`) |
| [ai-kullanim-gunlugu.md](docs/ai-kullanim-gunlugu.md) | Yapay zekâ araçlarının kullanımı |

## Haftalık ilerleme

| Hafta | Konu | Durum |
|---|---|---|
| 3 | Kurulum, mimari ve veri modeli | ✅ |
| 4 | Veritabanı ve veri erişim katmanı | ✅ |
| 5 | Kullanıcı yönetimi ve kimlik doğrulama | ✅ |
| 6 | Randevu takvimi ve yönetimi | ✅ |
| 7 | Belirti girişi ve temel yönlendirme | ✅ |
| 8 | Ara sürüm entegrasyonu ve vize hazırlık | ✅ |
| 9 | Harici API entegrasyonu: e-posta bildirim ve hatırlatma | ✅ |
| 10 | Yapay zekâ bileşeni: belirtiden bölüm yönlendirme (Claude) | ✅ |
| 11 | Hasta geçmişi modülü | ✅ |
| 12 | Randevu istatistikleri ve sıra panosu | ✅ |
| 13 | Test ve hata ayıklama (164 test, %89,7 satır kapsamı) | ✅ |
| 14 | Dokümantasyon, arayüz iyileştirmeleri, dağıtım hazırlığı (Docker, Render) | ✅ |
| 15 | Final entegrasyon, uçtan uca doğrulama, `v1.0.0` sürümü, demo ve sunum hazırlığı | ✅ |
