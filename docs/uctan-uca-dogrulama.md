# Uçtan Uca Doğrulama (Hafta 15)

Final sürümde her gereksinimin nerede ve nasıl doğrulandığı. "Otomatik" satırları her push'ta GitHub Actions'ta çalışır. "Elle" satırları sunumdan önce canlı adreste bir kez kontrol edilir.

## Ana akışlar
| # | Senaryo | Doğrulama | Türü |
|---|---|---|---|
| A1 | Giriş → belirti → öneri → randevu → doktor onayı → bildirim → geçmiş → erişim izi | `UctanUcaAkisTestleri` | Otomatik |
| A1b | Yeni hasta kaydı → giriş; şifre düz metin saklanmaz | `UctanUcaAkisTestleri` | Otomatik |
| A2 | Acil belirti kurallarının aciliyet seviyesi; yapay zekâ aciliyeti düşüremez | `KritikKurallarTestleri`, `BelirtiServisiYapayZekaTestleri` | Otomatik |
| A3 | Aynı slota ikinci randevu reddedilir; hasta aynı saatte başka bölümden de alamaz | `UctanUcaAkisTestleri`, `SinirDurumlariTestleri` | Otomatik |
| A4 | Hasta iptali → doktora bildirim → slot yeniden alınabilir | `UctanUcaAkisTestleri` | Otomatik |
| A5 | Her rolün her sayfası açılıyor (27 adres) | `SayfaTestleri` | Otomatik |

## Modüller
| Modül | Kontrol | Doğrulama | Türü |
|---|---|---|---|
| Kimlik doğrulama | Yanlış şifre, 5 denemede kilit, rol yönlendirmesi, anti-forgery | `KritikKurallarTestleri`, `SayfaTestleri` | Otomatik |
| Randevu | Slot sınırları (09:00, öğle arası, 17:00), geçmiş saat, durum geçişleri | `SinirDurumlariTestleri`, `KritikKurallarTestleri` | Otomatik |
| Yönlendirme | Puan hesabı, eşitlik, boş giriş | `KritikKurallarTestleri` | Otomatik |
| E-posta | Başarılı gönderim, 401, 429 + Retry-After, 5xx, zaman aşımı, anahtar yok | `BrevoEpostaGondericiTestleri`, `HatirlatmaVeKuyrukTestleri` | Otomatik (sahte HTTP) |
| E-posta (gerçek) | Yönetim → E-posta gönderimleri → **Test e-postası** | Gelen kutusu | Elle (anahtar girildiyse) |
| Yapay zekâ | Şema dışı yanıt, bilinmeyen bölüm, ret, hata, maskeleme, acil yükseltme | `YapayZeka/*` testleri | Otomatik (sahte istemci) |
| Yapay zekâ (gerçek) | Onay kutusu işaretli serbest metinle belirti girişi | Sonuç sayfasında YZ kutusu | Elle (anahtar girildiyse) |
| Hasta geçmişi | Başkasının geçmişi 404, ilişkisiz doktor 404, yöneticide tıbbi içerik gizli, erişim izi | `Gecmis/*` testleri, `SayfaTestleri` | Otomatik |
| İstatistik | Oranlar, sıfıra bölme, kapasite, CSV formül koruması | `Istatistik/*` testleri | Otomatik |
| Veritabanı | Migration'lar modelle uyumlu, boş veritabanına sırayla uygulanıyor | CI `has-pending-model-changes`, `MigrationTestleri` | Otomatik |

## Dağıtım
| Kontrol | Doğrulama | Türü |
|---|---|---|
| Docker imajı derleniyor ve açılıyor | CI `docker` işi | Otomatik |
| `/saglik` → Healthy | CI `docker` işi, `DagitimHazirligiTestleri` | Otomatik |
| Proxy arkasında HTTPS ile giriş sayfası, 404 sayfası, yetkisiz yönlendirme | CI `docker` işi (duman testi) | Otomatik |
| Saat dilimi (Europe/Istanbul) UTC sunucuda doğru | `SinirDurumlariTestleri` (H-03 regresyonu), imajda `tzdata` | Otomatik |
| Canlı adres açılıyor, demo hesabıyla giriş yapılıyor | Tarayıcı | Elle |
| Gizli anahtarlar depoda yok | `git grep` ile `sk-ant`, `xkeysib` araması → sonuç yok | Elle |

## Sunum öncesi kontrol listesi (canlı adres)
- [ ] Adresi sunumdan 2–3 dakika önce aç (ücretsiz plan uykudan uyanır).
- [ ] `/saglik` → Healthy.
- [ ] `hasta1`, `doktor3`, `yonetici` hesaplarıyla giriş.
- [ ] Belirti → öneri → randevu → doktor onayı akışı.
- [ ] İstatistik sayfası ve CSV indirme.
- [ ] (Anahtar girildiyse) yapay zekâ değerlendirmesi ve test e-postası.
- [ ] İnternet sorununa karşı yedek: yerelde Docker ya da ekran kaydı.
