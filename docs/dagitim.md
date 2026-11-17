# Dağıtım (Render)

Uygulama Docker imajı olarak [Render](https://render.com) üzerinde çalışır. Depodaki [`render.yaml`](../render.yaml) dosyası servisi tanımlar (Blueprint).

## Adımlar
1. render.com → **Get Started** → **GitHub ile giriş**.
2. **New → Blueprint** → `akilli-randevu-yonlendirme-asistani` deposunu seç.
3. Render `render.yaml` dosyasını okur ve gizli değerleri sorar. Boş bırakılanlar ilgili özelliği kapatır:
   | Değişken | Değer |
   |---|---|
   | `ANTHROPIC_API_KEY` | console.anthropic.com'dan alınan anahtar (isteğe bağlı) |
   | `Eposta__ApiAnahtari` | Brevo → SMTP & API → API Keys (isteğe bağlı) |
   | `Eposta__GondericiAdres` | Brevo'da doğrulanmış gönderici adresi |
   | `Eposta__YonlendirmeAdresi` | Demo e-postalarının gideceği kendi adresin |
4. **Apply**. İlk derleme birkaç dakika sürer. Bittiğinde adres `https://<servis-adi>.onrender.com` olur.
5. Kontrol: `https://<adres>/saglik` → `Healthy`. Ana sayfada `hasta1@ornek.test / Demo1234` ile giriş yap.
6. Adresi README'deki "Canlı demo" satırına yaz.

> **Anahtarlar yalnızca Render panelinde girilir.** Depoya, sohbete ya da ekran görüntüsüne konmaz. Yanlışlıkla paylaşılırsa sağlayıcının panelinden hemen iptal edilip yenisi oluşturulur.

## render.yaml ne yapıyor?
| Ayar | Değer | Neden |
|---|---|---|
| `runtime: docker` | Dockerfile | Aynı imaj CI'da test ediliyor |
| `plan: free`, `region: frankfurt` | | Ücretsiz; Türkiye'ye yakın bölge |
| `healthCheckPath: /saglik` | | Render uygulamanın ayakta olduğunu buradan anlar |
| `DemoModu=true` | | Sınav/demo için hazır hesaplar |
| `ProxyArkasinda=true` | | Render HTTPS'i kendisi sonlandırır; uygulama `X-Forwarded-Proto` başlığına güvenir, çerezler `Secure` kalır |

## Ücretsiz planın sınırları
- 15 dakika istek gelmezse servis uyur; ilk istek ~1 dakika sürebilir. **Sunumdan önce adresi bir kez açın.**
- Disk kalıcı değil: yeniden başlatma ya da yeni dağıtımda SQLite veritabanı sentetik veriye döner. Demo için bu bir avantaj (her sunum temiz başlar); gerçek kullanımda kalıcı disk ya da yönetilen veritabanı gerekir.
- Oturum anahtarları da sıfırlandığı için yeniden başlatmadan sonra tekrar giriş gerekir.

## Gerçek kullanıma geçmek için gerekenler
Bu dağıtım bir **demo**dur. Gerçek bir poliklinikte kullanılmadan önce:
- `DemoModu=false`, `IlkYonetici__*` ile güçlü şifreli yönetici; demo hesaplar silinmeli.
- Kalıcı veritabanı (PostgreSQL vb.) ve yedekleme.
- KVKK: veri sorumlusu kaydı, aydınlatma metninin hukukçu tarafından gözden geçirilmesi, yurt dışına veri aktarımı (Claude, Brevo) için açık rıza ve sözleşmeler.
- Yapay zekâ önerisinin klinik doğrulaması.

## Alternatif: GitHub Codespaces
Depo sayfası → **Code → Codespaces → Create**. Terminalde `dotnet run --project src/AkilliRandevu.Web` çalıştırılır; Codespaces geçici bir adres verir. Kalıcı link değildir, yalnızca deneme içindir.
