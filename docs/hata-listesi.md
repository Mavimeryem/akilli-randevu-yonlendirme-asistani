# Hata Listesi

## Düzeltilen hatalar
| No | Hafta | Hata | Belirti (nasıl fark edildi) | Kök neden | Düzeltme | Test |
|---|---|---|---|---|---|---|
| H-01 | 8 | Demo senaryosu yanlış sonuç bekliyordu | Göğüs ağrısı 6 + öksürük 7 → beklenen "Kardiyoloji", gerçek sonuç "Göğüs Hastalıkları %47" | Beklenen sonuç el hesabı yapılmadan yazılmıştı | Senaryo göğüs ağrısı + çarpıntı (Kardiyoloji %64) yapıldı, hesap belgeye eklendi | Yönlendirme motoru testleri |
| H-02 | 8 | Belgelerde "BCrypt" yazıyordu | Kod incelemesi | Kod PBKDF2-SHA256 kullanıyor | Belgeler düzeltildi | `Ayni_sifre_her_seferinde_farkli_hash_...` |
| **H-03** | 13 | Belirti kayıtlarım saati sunucu saat dilimine göre gösteriliyordu | Bulut sunucuya (UTC) hazırlık sırasında kod taraması | `ToLocalTime()` sunucunun saat dilimini kullanır | Poliklinik saatine (`ISaat.YereleCevir`) çevrildi | `Sunucu_UTCde_calissa_da_poliklinik_saati_Istanbul_saatidir` |
| H-04 | 9 | "Şimdi" sunucunun yerel saatinden alınıyordu | Aynı tarama | `TimeProvider.GetLocalNow()` | `Europe/Istanbul` sabit saat dilimli `PoliklinikSaati` | Aynı test + saat sabitlenmiş testler |
| H-05 | 6 | Bir randevunun durumunu güncellemek hasta, doktor ve bölüm satırlarını da güncelliyordu | Kod incelemesi | `DbSet.Update` bütün ilişkili nesneleri "değişti" olarak işaretler | Nesne zaten takip ediliyorsa yalnızca `SaveChanges` çağrılıyor | Durum değişikliği testleri |
| H-06 | 7 | İptal edilen randevunun belirti kaydından tekrar randevu alınamıyordu | Uçtan uca deneme | Bir belirti kaydı tek bir randevuya bağlanabiliyor (benzersiz indeks) | İptal edilen randevunun bağlantısı kaldırılıyor | 7. hafta senaryo testi |
| H-07 | 12 | İstatistik sayfası derlenmedi (RZ1023) | Derleme hatası | Razor, SVG'nin `<text>` öğesini kendi özel etiketi sandı | `@:` ile düz çıktı | Sayfa testi `/Istatistik` |
| H-08 | 10 | Yapay zekâ kartında süre bozuk görünecekti | Kod incelemesi (derleyici yakalamaz) | `@(x)"0.0"` yanlış Razor sözdizimi | `@(x.ToString("0.0"))` | Sayfa testi `/Belirti/Sonuc/1` |
| H-09 | 13 | Testlerde `.Result` kullanımı | CI uyarısı xUnit1031 | Senkron bekleme kilitlenmeye (deadlock) yol açabilir | `async` / `await` | – |
| H-10 | 13 | CI'da Node.js 20 uyarısı | CI uyarısı | Aksiyonların eski sürümü | `checkout@v7`, `setup-dotnet@v6`, `upload-artifact@v7` | CI |
| H-11 | 4 | Migration kontrolü "değişiklik var" diyordu | `has-pending-model-changes` | `--no-build` eski derlemeye bakıyordu (yanlış alarm) | Kontrol derlemeyle çalıştırıldı; CI'da derleme sonrası çalışıyor | `Tum_migrationlar_uygulanir_...` |
| H-12 | 11–12 | Belgelerde test sayıları yanlıştı (17 → 18, 18 → 16) | Test raporu için dosya dosya sayım | Sayılar tahminle yazılmıştı | Düzeltildi; sayılar artık CI çıktısından alınıyor | – |

## Adım adım ayıklama örneği: H-03 (saat dilimi)
1. **Belirti:** Proje yerelde doğru çalışıyordu. Canlıya alma hazırlığında "bulut sunucu UTC'de çalışır, saate bağlı ne bozulur?" sorusuyla kod tarandı: `ToLocalTime`, `DateTime.Now`, `DateTime.Today`, `GetLocalNow`.
2. **Bulgu:** `Views/Belirti/Kayitlarim.cshtml` içinde `k.OlusturmaTarihi.ToLocalTime()` vardı.
3. **Yeniden üretme (zihinde ve testle):** Kayıt `2026-10-14 08:00 UTC` olarak saklanıyor. Türkiye saatinde çalışan bir bilgisayarda `ToLocalTime()` → 11:00 (doğru). UTC sunucuda `ToLocalTime()` → 08:00 (**3 saat geri**). Ayrıca SQLite tarihleri `Kind = Unspecified` olarak döndürür; `ToLocalTime()` bunu UTC kabul eder, bu da sonucu sunucuya göre daha da tutarsız yapar.
4. **Kök neden:** Gösterim için **sunucunun** saat dilimi kullanılıyordu. Doğrusu **polikliniğin** saat dilimi (Europe/Istanbul).
5. **Düzeltme:** Görünüm `ISaat.YereleCevir(...)` kullanıyor. Bu servis saat dilimini ayardan (`Takvim:SaatDilimi`) alıyor ve sunucudan bağımsız.
6. **Regresyon testi:** `Sunucu_UTCde_calissa_da_poliklinik_saati_Istanbul_saatidir`. Saat UTC 06:30'a sabitlendiğinde "şimdi" 09:30 olmalı, UTC 21:30 ertesi gün 00:30'a çevrilmeli. Test CI'da (UTC Linux sunucusunda) çalışıyor; hata geri gelirse test kırılır.
7. **Ders:** Zamanı her zaman UTC sakla, gösterirken **iş kuralının** saat dilimine çevir; sunucunun saat dilimine asla güvenme.

## Bilinen sınırlar (açık)
| No | Konu | Etki | Plan / not |
|---|---|---|---|
| B-01 | Giriş deneme sayaçları bellekte | Uygulama yeniden başlayınca sıfırlanır; birden çok sunucuda paylaşılmaz | Tek sunucu için yeterli; büyürse veritabanı ya da Redis |
| B-02 | Sentetik veri migration içinde | Gerçek kurulumda da yüklenir (girişleri kapalıdır) | Gerçek kullanımda örnek verisiz ayrı bir migration gerekir |
| B-03 | Ücretsiz Render planında disk kalıcı değil | Yeniden dağıtımda SQLite sıfırlanır, örnek veri geri gelir | Demo için kabul edilebilir; kalıcılık için ücretli disk ya da PostgreSQL |
| B-04 | Migration'lar elle yazıldı | Bu bilgisayarda `dotnet ef` Windows Uygulama Denetimi'ne takılıyor | CI her push'ta model uyumunu ve migration'ları doğruluyor |
| B-05 | Tahmini bekleme planlanan saate göre hesaplanıyor | Muayene gecikmeleri hesaba katılmıyor | Hasta "geldi" kaydı (check-in) eklenirse iyileşir |
| B-06 | Bildirim kanalı yalnızca e-posta | SMS yok | Kanal soyutlaması hazır; yeni bir `IEpostaGonderici` benzeri eklenebilir |
| B-07 | Geliştirme bilgisayarında uygulama çalışmıyor | Windows Akıllı Uygulama Denetimi derlenen DLL'leri engelliyor | Testler CI'da, demo canlı sunucuda |
