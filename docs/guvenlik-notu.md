# Güvenlik Notu – Kimlik Doğrulama ve Yetkilendirme

## Kimlik doğrulama ve yetkilendirme
- **Kimlik doğrulama (authentication):** *"Sen kimsin?"* sorusunun cevabı. E-posta ve şifre kontrol edilir, kullanıcıya oturum açılır (`HesapController.Giris`).
- **Yetkilendirme (authorization):** *"Bunu yapmaya iznin var mı?"* sorusunun cevabı. Kullanıcının rolü kontrol edilir (`[Authorize(Roles = ...)]`).

## 1. Şifre saklama
| Konu | Uygulama |
|---|---|
| Algoritma | PBKDF2-HMAC-SHA256, 600.000 tekrar (OWASP önerisi) – `Pbkdf2SifreHasher` |
| Tuz (salt) | Her şifre için 16 bayt rastgele (`RandomNumberGenerator`) |
| Saklanan değer | `PBKDF2-SHA256$600000$<tuz>$<hash>` – düz şifre hiçbir yerde tutulmaz |
| Karşılaştırma | `CryptographicOperations.FixedTimeEquals` (sabit süreli, timing saldırısına karşı) |
| Şifre kuralı | En az 8 karakter, en az bir harf ve bir rakam |

**Neden hash?** Veritabanı çalınsa bile şifreler doğrudan okunamaz. Hash tek yönlüdür, geri çevrilemez.
**Tuz neden var?** Aynı şifreyi kullanan iki kişinin hash'i farklı çıkar. Hazır hash tabloları (rainbow table) işe yaramaz, çünkü saldırgan her kullanıcı için ayrı ayrı uğraşmak zorunda kalır.
**Neden yavaş bir algoritma?** SHA-256 tek başına çok hızlıdır ve saniyede milyarlarca deneme yapılabilir. PBKDF2'nin 600.000 tekrarı her denemeyi pahalı hale getirir. bcrypt ve Argon2 da aynı amaçla kullanılan alternatiflerdir. PBKDF2'yi seçtim çünkü .NET'in içinde hazır geliyor ve ek paket gerektirmiyor.

## 2. Giriş akışı ve başarısız denemeler
1. E-posta küçük harfe çevrilir, kullanıcı aranır.
2. Kullanıcı bulunamasa bile sahte bir hash ile doğrulama yapılır. Böylece yanıt süresinden "bu e-posta kayıtlı mı" anlaşılamaz.
3. Hata mesajı her durumda aynıdır: **"E-posta veya şifre hatalı."** Hangisinin yanlış olduğu söylenmez.
4. Aynı e-postaya **5 başarısız denemeden sonra giriş 15 dakika kilitlenir** (`BellekGirisDenemeTakipcisi`). Bu, kaba kuvvet (brute force) saldırısına karşı bir önlemdir.
5. Başarılı girişte sayaç sıfırlanır. Pasif hesap giriş yapamaz.

Bilinen sınır: Sayaçlar bellekte tutulur, uygulama yeniden başlarsa sıfırlanır. Birden çok sunucuya geçildiğinde veritabanına ya da Redis'e taşınmalıdır.

## 3. Oturum yönetimi (çerez)
| Ayar | Değer | Amaç |
|---|---|---|
| `HttpOnly` | açık | JavaScript çereze erişemez, XSS ile çalınamaz |
| `SameSite` | Lax | Başka sitelerden gelen POST isteklerinde çerez gönderilmez |
| `Secure` | canlıda her zaman | Çerez yalnızca HTTPS üzerinden gider |
| Süre | 30 dk, kayan (sliding) | İşlem yapılmazsa oturum kendiliğinden kapanır |
| Çıkış | POST + anti-forgery | Bir link ile kullanıcı zorla çıkış yaptırılamaz |
| Anti-forgery | tüm POST'larda (`AutoValidateAntiforgeryToken`) | CSRF koruması |
| Çerez içeriği | ASP.NET Data Protection ile şifreli ve imzalı | Kullanıcı rolünü değiştiremez |

### Oturum (session/cookie) ve token (JWT) karşılaştırması
| | Çerez oturumu (bu projede) | JWT |
|---|---|---|
| Nerede durur | Tarayıcı çerezi, tarayıcı otomatik gönderir | İstemci saklar, `Authorization: Bearer` başlığıyla gönderir |
| Uygun olduğu yer | Sunucuda sayfa üreten web uygulaması (MVC) | Mobil uygulama, SPA, servisler arası API |
| Çıkış / iptal | Çerez silinir | Token süresi dolana kadar geçerli kalır, iptal için ek mekanizma gerekir |
| Risk | CSRF (SameSite ve anti-forgery ile önlenir) | Tarayıcıda saklanırsa XSS ile çalınabilir |

Bu proje MVC olduğu için çerez oturumu seçildi. İleride mobil uygulama ya da API eklenirse JWT kullanılabilir.

## 4. Rol tabanlı erişim
| Rol | Erişebildiği sayfa |
|---|---|
| Hasta | `/Hasta/Randevularim` |
| Doktor | `/Doktor/Takvim` |
| Yönetici | `/Yonetim/Kullanicilar` |

- Kontrol **sunucu tarafında** yapılır: controller'larda `[Authorize(Roles = ...)]` var. Menüde linki gizlemek sadece kolaylık sağlar, güvenlik sağlamaz.
- Giriş yapmamış kullanıcı korumalı bir sayfaya giderse **giriş sayfasına** yönlenir (`/Hesap/Giris?ReturnUrl=...`). Giriş yaptıktan sonra gitmek istediği sayfaya döner.
- Giriş yapmış ama rolü yetmeyen kullanıcı **"Erişim engellendi"** sayfasına yönlenir (HTTP 403).
- Hasta ve doktor kimliği URL'den değil **oturumdan** okunur. Böylece başka hastanın randevuları görüntülenemez (IDOR koruması).
- Kayıt formundan sadece **Hasta** hesabı açılabilir. Rol formdan alınmaz, kodda sabittir.
- `ReturnUrl` yalnızca site içi bir adresse kullanılır (open redirect koruması).

## OWASP Top 10 ile ilişkisi
| OWASP | Bu projede |
|---|---|
| A01 Broken Access Control | Sunucu tarafı rol kontrolü, kimliğin oturumdan alınması, ReturnUrl kontrolü |
| A02 Cryptographic Failures | Tuzlu PBKDF2, HTTPS ve Secure çerez |
| A03 Injection | EF Core parametreli sorgular, Razor'ın otomatik HTML kodlaması |
| A07 Identification and Authentication Failures | Deneme sınırı, genel hata mesajı, şifre kuralı, oturum süresi |

## Geliştirme ortamı test hesapları
Sentetik kullanıcılar migration'da `SENTETIK-HASH` yer tutucusuyla gelir. Bu geçerli bir hash olmadığı için bu haliyle giriş yapılamaz. **Sadece geliştirme ortamında**, uygulama açılırken bu değer `Demo1234` şifresinin gerçek hash'iyle değiştirilir ve her kullanıcı ayrı bir tuz alır. Canlı ortamda bu adım çalışmaz.
