# Değişiklik Günlüğü

Sürüm numaraları [Anlamsal Sürümleme](https://semver.org/lang/tr/) kurallarına uyar.

## [1.0.0] – 2026-11-17 · Final sürüm
### Eklendi
- **H9 – Bildirim:** uygulama içi bildirimler; Brevo REST API ile e-posta (giden kutusu deseni, tekrar deneme, 429/5xx yönetimi); randevudan 24 saat önce hatırlatma; arka plan servisi; anahtar ortam değişkeninde.
- **H10 – Yapay zekâ:** Claude API ile serbest metinden bölüm önerisi, gerekçe ve güven düzeyi; JSON şeması ve sunucu tarafı doğrulama; kişisel veri maskeleme; açık onay; kural tabanlı öneriye geri dönüş; her çağrının kaydı.
- **H11 – Hasta geçmişi:** randevu, belirti ve not zaman çizelgesi; rol ve ilişkiye göre erişim; tarih/tür/durum/bölüm filtreleri; erişim izi; doktor muayene notu.
- **H12 – İstatistik ve sıra panosu:** bölüm yoğunluğu, doluluk, iptal ve gelmeme oranları, gün/saat ısı haritası, bekleme süresi; CSV dışa aktarma; maskeli adlarla canlı sıra panosu.
- **H13 – Test:** 164 test (birim, sınır, bileşen, web, uçtan uca); kapsam raporu; hata listesi.
- **H14 – Dağıtım ve belgeler:** Dockerfile, Render Blueprint, `/saglik`, proxy arkasında HTTPS, kalıcı oturum anahtarları, demo modu; erişilebilirlik iyileştirmeleri; Türkçe hata ve gizlilik sayfaları; README ve teknik dokümantasyon.
- **H15 – Final:** uçtan uca doğrulama listesi, final demo senaryosu, sunum taslağı, `v1.0.0` etiketi.

### Düzeltildi
- H-03: Hasta belirti kayıtlarında saat, sunucunun saat dilimine göre gösteriliyordu (bulutta 3 saat kayma). Ayrıntılar: [docs/hata-listesi.md](docs/hata-listesi.md).
- Hafta 8 belgelerindeki yüzde hesabı ve şifre algoritması adı düzeltildi.

## [0.8.0] – 2026-11-12 · Ara sürüm (vize)
### Eklendi
- **H3–H4:** katmanlı mimari, EF Core + SQLite veri modeli, migration'lar, sentetik veri.
- **H5:** kayıt, giriş, PBKDF2 şifre hash'i, rol tabanlı yetki, giriş kilidi.
- **H6:** çalışma saatlerine göre slot üretimi, çakışma kontrolü, randevu durum makinesi.
- **H7:** belirti girişi, ağırlıklı puanla bölüm önerisi, acil durum uyarısı.
- **H8:** modüllerin entegrasyonu, demo senaryosu, ilerleme raporu.
