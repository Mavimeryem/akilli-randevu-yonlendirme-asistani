# Randevu İstatistikleri ve Sıra Panosu (Hafta 12)

## Ekranlar
| Sayfa | Kim | İçerik |
|---|---|---|
| **İstatistik** (`/Istatistik`) | Yönetici | Tarih aralığı ve bölüm filtresi, hazır aralıklar (son 7/30/90 gün, bu ay, önümüzdeki 14 gün), 4 özet sayı, bölüm doluluğu, gün × saat yoğunluğu, günlük grafik + tablo görünümü, iki CSV dışa aktarma |
| **Sıra panosu** (`/SiraPanosu`) | Doktor, yönetici | Bugün her doktor için: şu an muayenedeki hasta, sıradaki hasta, tahmini bekleme, kalan / tamamlanan / gelmeyen. Sayfa her dakika yenilenir |

## Metrikler ve hesap mantığı (`Business/Istatistik/IstatistikHesaplayici.cs`)
Hesap mantığı veritabanından ayrı, **saf** bir sınıftadır; bu yüzden elle hazırlanmış verilerle test edilebiliyor.

| Metrik | Tanım | Neden bu metrik |
|---|---|---|
| Toplam randevu | Aralıkta başlayan bütün randevular (iptaller dahil) | Talep hacmi |
| Aktif randevu | İptal edilmemiş randevular | Gerçek iş yükü |
| **Kapasite** | Aktif doktor sayısı × aralıktaki çalışma günü × günlük slot sayısı (çalışma düzeninden, hafta sonu ve öğle arası hariç) | Doluluğun paydası; çalışma saatleri değişince kendiliğinden doğru hesaplanır |
| **Doluluk** | Aktif ÷ kapasite | Hangi bölümde yeni doktora ya da slota ihtiyaç var? |
| **İptal oranı** | İptal ÷ toplam | Planlama kaybı |
| **Gelmeme oranı** | Gelmedi ÷ (tamamlandı + gelmedi) | Boşa giden slot; hatırlatmanın (H9) etkisini ölçmek için |
| **Yoğunluk** | Gün × saat başına aktif randevu | En yoğun gün ve saatleri görmek |
| Günlük trend | Gün başına aktif ve iptal sayısı | Zaman içindeki değişim |

### Eksik ve yanlış veri sonucu nasıl etkiler?
- **Sonucu girilmemiş randevular:** Saati geçtiği halde hâlâ "beklemede" ya da "onaylı" görünen randevular, gelmeme oranının paydasına **katılmaz**, ayrıca sayılır. Ekranda uyarı çıkar: "X randevunun sonucu girilmemiş; bu kayıtlar girilince oran değişebilir." Bu kayıtlar "geldi" ya da "gelmedi" kabul edilseydi oran yanıltıcı olurdu.
- **Sıfıra bölme:** Payda 0 olduğunda, örneğin sadece hafta sonunu kapsayan bir aralıkta kapasite 0'dır; oran **"—"** gösterilir. "%0" demek "hiç dolu değil" anlamına gelir ve yanlış olurdu.
- **Pasif doktor:** Kapasiteye katılmaz. Geçmişteki randevuları yine sayılır.
- **Özet ve filtre ayrı:** Bölüm filtresi hem randevulara hem kapasiteye uygulanır, böylece oranlar tutarlı kalır.

## Sıra panosu
- **Şu an:** Başlamış ama bitmemiş, sonucu girilmemiş randevu.
- **Sıradaki:** Bugün, şu andan sonra başlayan ilk bekleyen ya da onaylı randevu.
- **Tahmini bekleme:** Sıradaki randevunun planlanan saati eksi şu an (dakika). Muayene gecikmeleri hesaba katılmaz; sayfada bu not yazılıdır.
- **Gizlilik:** Pano bir bekleme salonu ekranında gösterilebileceği için hasta adları **maskelenir**: "Deniz Acar" → "D*** A***".
- **Saat:** Poliklinik saatine (`Europe/Istanbul`) göre hesaplanır, sunucunun saat diliminden bağımsızdır.

## Görselleştirme kararları
| Veri | Biçim | Neden |
|---|---|---|
| 4 özet sayı | Sayı kutuları (grafik değil) | Tek bir değer için grafik gereksiz |
| Bölüm doluluğu | Satır başına çubuk (bölüm adı + çubuk + yüzde) | Her bölüm kendi kapasitesine göre okunur; pasta grafik bölümleri karşılaştırmayı zorlaştırır |
| Gün × saat | Isı haritası; tek renk, açıktan koyuya; hücrede sayı yazılı | Büyüklük için tek ton yeterli, gökkuşağı renkleri yanıltır |
| Günlük trend | İnce sütunlar, ızgara çizgileri soluk, üzerine gelince değer görünür | Tek seri olduğu için lejant yok, başlık seriyi anlatıyor |

- Renkler doğrulanmış bir paletten geliyor (mavi `#2a78d6` ve tonları). Tek seri ve tek ton kullanıldığı için renk körlüğünde sorun çıkmaz.
- Hiçbir bilgi **yalnızca renkle** verilmez: ısı haritasında sayı yazılı, panoda durumlar ● / ○ / ✓ / ✗ işaretleri ve metinle gösteriliyor.
- **Tablo görünümü:** Günlük grafiğin verisi "Tablo olarak göster" ile açılabiliyor (ekran okuyucu ve erişilebilirlik için).
- Grafikler dış kütüphane olmadan sunucuda SVG ve HTML olarak üretilir; sayfa internet bağlantısı olmadan da çalışır.

## Dışa aktarma (CSV)
| Dosya | İçerik |
|---|---|
| `bolum-istatistikleri_*.csv` | Bölüm, toplam, aktif, kapasite, doluluk %, iptal %, gelmeme % |
| `randevular_*.csv` | Randevu no, tarih, saat, bölüm, doktor, durum. **Hasta adı yok** (veri minimizasyonu) |

- Biçim Türkçe Excel'e uygun: ayraç `;`, UTF-8 (BOM ile), böylece Türkçe karakterler bozulmaz.
- **CSV enjeksiyonuna karşı:** `=`, `+`, `-` ya da `@` ile başlayan hücreler Excel'de formül olarak çalışabilir. Bu hücrelerin başına `'` eklenir.

## Diğer modüllerle entegrasyon
Randevu (H6) verisi ve durumları, çalışma düzeni ve slotlar (`TakvimAyarlari`, `SlotUretici`), poliklinik saati (H9 `ISaat`), roller (H5: istatistik sadece yönetici, pano personel).

## Testler (`tests/AkilliRandevu.Tests/Istatistik/IstatistikTestleri.cs`)
Doluluk, iptal ve gelmeme oranı; sonuçlanmamış randevuların dışarıda bırakılması; sıfıra bölme; yoğunluk kovaları; örnek veriyle Ekim 2026 (10 randevu, kapasite 10 × 22 × 21, gelmeme %25); tarih aralığı doğrulama; sıra panosu (şu an, sıradaki, bekleme, maskeleme); CSV kaçışları, formül koruması, UTF-8 BOM, hasta adının dışa aktarılmaması.
