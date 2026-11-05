# Belirti Girişi ve Temel Yönlendirme Modülü

Hasta belirtilerini ve şiddetlerini girer. Sistem **kural tabanlı** bir ön değerlendirme yapar ve hangi polikliniğe başvurulabileceğini önerir. **Tanı koymaz.**

## Veri modeli

| Tablo | Rolü |
|---|---|
| `Belirtiler` | Belirti kataloğu (16 belirti) |
| `BelirtiBolumleri` | Belirti ↔ Bölüm (N-N), `AgirlikPuani` 1–10: belirti bu bölüme ne kadar işaret ediyor |
| `BelirtiKayitlari` | Hastanın bir girişi: açıklama, hesaplanan `Aciliyet`, `OnerilenBolumId` |
| `BelirtiKaydiBelirtileri` | Kayıt ↔ Belirti (N-N), `Siddet` 1–10 |
| `Randevular.BelirtiKaydiId` | Kayıttan alınan randevu (1–0..1, benzersiz) |

Bu hafta yeni tablo eklenmedi. Model 3. ve 4. haftalarda bu modül düşünülerek kurulmuştu.

## Yönlendirme kuralları (`Business/Yonlendirme/YonlendirmeMotoru.cs`)

Kural motoru saf bir fonksiyondur: veritabanına gitmez ve aynı girdiye her zaman aynı sonucu verir.

### 1. Bölüm puanı
```
puan(bölüm) = Σ  AgirlikPuani(belirti, bölüm) × Siddet(belirti)
yüzde       = puan(bölüm) / Σ puan × 100
```
En yüksek puanlı bölüm önerilir. İlk 3 bölüm yüzdeleriyle gösterilir.
Örnek: Bel ağrısı (şiddet 8) için Ortopedi 9×8 = 72, Nöroloji 4×8 = 32 puan alır. Sonuç: **Ortopedi %69**, Nöroloji %31.

### 2. Belirsizlik (tanı olmadığının gösterilmesi)
Aşağıdaki durumlarda sonuç **belirsiz** sayılır ve ilk sıraya **Dahiliye** (genel ilk değerlendirme) konur:
- Hiçbir bölüm eşleşmezse,
- En uygun bölümün payı **%40'ın altındaysa**,
- İlk iki bölüm arasındaki fark **10 puandan azsa**.

### 3. Aciliyet (kırmızı bayrak kuralları)
Başlangıç seviyesi en yüksek şiddete göre belirlenir: şiddet 7 ve üstüyse **Orta**, değilse **Düşük**. Aşağıdaki kurallardan biri tutarsa seviye yükseltilir:

| Kural | Seviye |
|---|---|
| Göğüs ağrısı şiddeti ≥ 7 | Acil |
| Göğüs ağrısı + nefes darlığı | Acil |
| Nefes darlığı şiddeti ≥ 8 | Acil |
| Uyuşma + (bulanık görme veya baş dönmesi): inme şüphesi | Acil |
| Ateş ≥ 7 + baş ağrısı ≥ 7 | Yüksek |
| Karın ağrısı ≥ 8 | Yüksek |
| Çarpıntı + baş dönmesi | Yüksek |

**Acil** durumda bölüm önerilmez ve randevu verilmez. Kullanıcı 112'ye ve acil servise yönlendirilir.

### 4. Bilgilendirme metni
Her sonucun sonunda şu metin yer alır: *"Bu sonuç bir tıbbi tanı değildir… kesin değerlendirmeyi hekiminiz yapar."* Aciliyete ve belirsizliğe göre ek açıklamalar eklenir. Giriş formunda da acil durumlarda formun doldurulmaması, 112'nin aranması gerektiği yazar.

## Doğrulama ve iş kuralları (`BelirtiService`)
| Kural | Neden |
|---|---|
| En az 1, en fazla 8 belirti | Boş giriş anlamsız. Her şeyi işaretlemek puanları sulandırır |
| Aynı belirti iki kez seçilemez | Puan iki kez sayılmasın |
| Şiddet 1–10 | Veritabanında da CHECK kısıtı var |
| Belirti Id'leri katalogda olmalı | Formdan sahte Id gönderilemesin |
| Açıklama en fazla 1000 karakter | Sütun sınırı |
| Son 24 saatte en fazla 10 kayıt | Kötüye kullanım sınırı |
| Hasta yalnızca kendi kaydını görebilir (başkasınınkine 404) | Sahiplik. 404 verilir ki kaydın var olup olmadığı anlaşılmasın |

## Entegrasyon
| Modül | Bağlantı |
|---|---|
| Kimlik doğrulama | Sayfalar yalnızca Hasta rolüne açık, hasta kimliği oturumdan alınıyor |
| Randevu (Hafta 6) | Sonuç sayfasındaki "Randevu al" butonu, bölüm seçili ve kayıt bağlı şekilde randevu ekranını açar. `OlusturAsync` kontrolleri: kayıt hastanın mı, acil mi, zaten aktif bir randevuya bağlı mı. İptal edilen randevunun bağlantısı kaldırılır ve aynı kayıttan yeniden randevu alınabilir |
| Doktor takvimi | Doktor, randevudan önce hastanın belirtilerini, şiddetlerini, aciliyeti ve açıklamasını görür |
| Hasta geçmişi | "Belirti kayıtlarım" sayfası: tarih, belirtiler, aciliyet, önerilen bölüm ve bağlı randevunun durumu |

## Bilinen sınırlar
- Kurallar sabit ve basit. Gerçek bir triyaj sistemi hekimlerle birlikte doğrulanmalıdır.
- Acil kurallarındaki belirti Id'leri `BelirtiKodlari` içinde sabit tutuluyor ve katalogla birlikte güncellenmeli.
- Sonuç sayfası, kayıttaki belirtilerden yönlendirmeyi yeniden hesaplar. Ağırlıklar değişirse eski kayıtların öneri listesi de değişebilir. Kayıtta saklanan `OnerilenBolumId` ve `Aciliyet` değerleri ise değişmez.
