# Teknik Dokümantasyon

## 1. Genel bakış
ASP.NET Core MVC (.NET 10) web uygulaması. Katmanlar: Web → Business → DataAccess (EF Core + SQLite) → Entities. Mimari şema: [mimari.md](mimari.md). Veri modeli: [ER-diyagrami.md](ER-diyagrami.md).

## 2. Modüller
| Modül | Hafta | Belge | Ana sınıflar |
|---|---|---|---|
| Kimlik doğrulama ve roller | 5 | [guvenlik-notu.md](guvenlik-notu.md) | `KullaniciService`, `Pbkdf2SifreHasher`, `BellekGirisDenemeTakipcisi`, `HesapController` |
| Randevu takvimi ve yönetimi | 6 | [randevu-modulu.md](randevu-modulu.md) | `RandevuService`, `SlotUretici`, `RandevuDurumKurallari` |
| Belirti girişi ve kural tabanlı yönlendirme | 7 | [belirti-yonlendirme.md](belirti-yonlendirme.md) | `BelirtiService`, `YonlendirmeMotoru` |
| E-posta bildirimi ve hatırlatma | 9 | [bildirim-entegrasyonu.md](bildirim-entegrasyonu.md) | `BrevoEpostaGonderici`, `EpostaKuyrugu`, `HatirlatmaServisi` |
| Yapay zekâ ile bölüm önerisi | 10 | [yapay-zeka-bileseni.md](yapay-zeka-bileseni.md) | `BolumOneriAsistani`, `YzCiktiDogrulayici`, `ClaudeYapayZekaIstemcisi` |
| Hasta geçmişi | 11 | [hasta-gecmisi.md](hasta-gecmisi.md) | `HastaGecmisiService` |
| İstatistik ve sıra panosu | 12 | [istatistik-ve-sira-panosu.md](istatistik-ve-sira-panosu.md) | `IstatistikHesaplayici`, `IstatistikService` |
| Test | 13 | [test-raporu.md](test-raporu.md), [hata-listesi.md](hata-listesi.md) | `tests/AkilliRandevu.Tests` |

## 3. HTTP uç noktaları
Projenin dışarıya açık bir JSON API'si yok; tüm işlevler sunucu tarafında üretilen sayfalardan kullanılır. Bütün POST isteklerinde anti-forgery token zorunlu.

| Yöntem | Adres | Rol | İşlev |
|---|---|---|---|
| GET | `/` | herkes | Ana sayfa, rolüne göre hızlı erişim |
| GET | `/Home/Gizlilik` | herkes | Gizlilik ve KVKK aydınlatma metni |
| GET | `/saglik` | herkes | Sağlık kontrolü ("Healthy") |
| GET/POST | `/Hesap/Giris`, `/Hesap/Kayit` | anonim | Giriş, hasta kaydı |
| POST | `/Hesap/Cikis` | giriş yapmış | Çıkış |
| GET/POST | `/Belirti/Giris` | Hasta | Belirti girişi (+ isteğe bağlı yapay zekâ onayı) |
| GET | `/Belirti/Sonuc/{id}` | Hasta (sahibi) | Yönlendirme sonucu |
| GET | `/Belirti/Kayitlarim` | Hasta | Belirti kayıtları |
| GET/POST | `/Randevu/Al` | Hasta | Bölüm → doktor → gün → boş saat; randevu talebi |
| POST | `/Randevu/DurumDegistir` | giriş yapmış (kurala göre) | Onay, iptal, tamamlandı, gelmedi |
| GET | `/Hasta/Randevularim` | Hasta | Randevular, iptal |
| GET | `/Gecmis?hastaId=&baslangic=&bitis=&tur=&durum=&bolumId=&sayfa=` | Hasta (kendi), Doktor (ilişkili), Yönetici | Hasta geçmişi |
| GET | `/Gecmis/Hastalarim` | Doktor | Randevusu olan hastalar |
| POST | `/Gecmis/DoktorNotu` | Doktor (randevunun sahibi) | Muayene notu |
| GET | `/Gecmis/ErisimKayitlari` | Yönetici | Erişim izleri |
| GET | `/Doktor/Takvim?gun=` | Doktor | Günlük slot takvimi |
| GET | `/SiraPanosu?bolumId=` | Doktor, Yönetici | Bugünkü sıra, maskeli adlar |
| GET | `/Istatistik?baslangic=&bitis=&bolumId=` | Yönetici | İstatistikler |
| GET | `/Istatistik/BolumlerCsv`, `/Istatistik/RandevularCsv` | Yönetici | CSV dışa aktarma |
| GET | `/Yonetim/Randevular`, `/Yonetim/Kullanicilar`, `/Yonetim/EpostaGonderimleri` | Yönetici | Yönetim ekranları |
| POST | `/Yonetim/TestEpostasi` | Yönetici | E-posta API'sini dener |
| GET/POST | `/Bildirim`, `/Bildirim/OkunduYap` | giriş yapmış | Uygulama içi bildirimler |

Yetkisiz isteklerin sonucu: giriş yapılmamışsa `302 → /Hesap/Giris`, rol yetmiyorsa `302 → /Hesap/ErisimEngellendi`, başkasına ait bir kayıt istenirse `404`.

## 4. Dış servisler
| Servis | Kullanım | Ayar | Servise ulaşılamazsa |
|---|---|---|---|
| **Claude API** (Anthropic, resmi C# SDK, `claude-opus-5-5`) | Belirti metninden bölüm önerisi | `ANTHROPIC_API_KEY`, `YapayZeka:*` | Kural tabanlı öneri geçerli kalır; hata kaydedilir |
| **Brevo** (REST `POST /v3/smtp/email`) | E-posta bildirimleri | `Eposta__ApiAnahtari`, `Eposta__GondericiAdres` | Uygulama içi bildirim gösterilir, e-posta tekrar denenir |

## 5. Arka plan işleri
`BildirimArkaPlanServisi` her `Bildirim:TurAraligiSaniye` (60) saniyede bir çalışır: hatırlatmaları oluşturur ve giden kutusundaki e-postaları gönderir. Bir turdaki hata uygulamayı durdurmaz.

## 6. Yapılandırma ve ortam değişkenleri
ASP.NET Core'da `Bolum:Alan` ayarı ortam değişkeni olarak `Bolum__Alan` yazılır.

| Ayar | Varsayılan | Açıklama |
|---|---|---|
| `ConnectionStrings__Varsayilan` | `Data Source=akilli_randevu.db` | SQLite dosyası (Docker'da `/app/veri/akilli_randevu.db`) |
| `DemoModu` | `false` (geliştirmede her zaman açık) | Sentetik hesaplar `Demo1234` ile girer; sayfa altında demo uyarısı çıkar |
| `IlkYonetici__Eposta`, `IlkYonetici__Sifre` | – | Verilirse bu e-postayla yönetici hesabı oluşturulur (yoksa) |
| `ProxyArkasinda` | `false` (Docker'da `true`) | HTTPS'i sonlandıran proxy arkasında `X-Forwarded-*` başlıklarına güven |
| `VeriKoruma__AnahtarKlasoru` | – (Docker'da `/app/veri/anahtarlar`) | Oturum çerezi anahtarlarının kalıcı klasörü |
| `ANTHROPIC_API_KEY` | – | Claude API anahtarı. **Gizli**; boşsa yapay zekâ kapalı |
| `YapayZeka__Model`, `__Efor`, `__ZamanAsimiSaniye`, `__YedekModelEtkin` | `claude-opus-5-5`, `low`, 45, `true` | Yapay zekâ ayarları |
| `Eposta__ApiAnahtari` | – | Brevo API anahtarı. **Gizli**; boşsa e-posta kapalı |
| `Eposta__GondericiAdres` | – | Brevo'da doğrulanmış gönderici |
| `Eposta__YonlendirmeAdresi` | – | Demo: bütün e-postaların gideceği adres |
| `Bildirim__ArkaPlanEtkin`, `__TurAraligiSaniye`, `__HatirlatmaSaatOnce` | `true`, 60, 24 | Bildirim servisi |
| `Takvim__*` | 09:00–17:00, öğle 12–13, 20 dk, `Europe/Istanbul` | Çalışma düzeni |

**Gizli anahtarlar depoda değildir.** `appsettings.json` içinde ilgili alanlar boştur. Değerler yerelde `dotnet user-secrets`, sunucuda platformun ortam değişkenleri ekranından verilir.

## 7. Veritabanı
- EF Core migration'ları: `IlkSema` (H4), `Hafta9Bildirim`, `Hafta10YapayZeka`, `Hafta11ErisimKaydi`. Uygulama açılırken bekleyen migration'lar otomatik uygulanır.
- Sentetik örnek veri (`SentetikVeri.cs`): 8 bölüm, 16 belirti, 10 doktor, 8 hasta, 10 randevu. Gerçek kişisel veri yok.
- 9. haftadan sonraki migration'lar, geliştirme bilgisayarında `dotnet ef` çalışmadığı için elle yazıldı. Doğrulukları CI'da `has-pending-model-changes` ve migration testleriyle kontrol ediliyor.

## 8. Güvenlik özeti
Tuzlu PBKDF2 parola hash'i; 5 hatalı girişte 15 dk kilit; HttpOnly, SameSite ve Secure çerez; bütün POST'larda anti-forgery; sunucu tarafında rol ve sahiplik kontrolü; yetkisiz isteğe 404; erişim izi; yöneticide tıbbi içerik gizli; CSV enjeksiyonu koruması; yapay zekâya giden metinde kişisel veri maskeleme; gizli anahtarlar ortam değişkeninde. Ayrıntılar: [guvenlik-notu.md](guvenlik-notu.md).
