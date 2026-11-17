# Hasta Geçmişi Modülü (Hafta 11)

Hastanın randevularını, belirti girişlerini, muayene notlarını ve yapay zekâ önerilerini **tek bir zaman çizelgesinde** toplar. Kimin hangi geçmişi görebileceği iş katmanında belirlenir. Hasta dışında birinin her erişimi kayıt altına alınır.

## Veri modeli
Geçmiş için ayrı bir "geçmiş" tablosu **tutulmaz**. Olaylar mevcut tablolardan (`Randevular`, `BelirtiKayitlari`, `YzDegerlendirmeleri`) okunur ve `GecmisOlayi` modelinde birleştirilir. Böylece aynı veri iki yerde tutulmaz ve tutarsızlık oluşmaz.

| Yapı | Nerede | İçerik |
|---|---|---|
| `GecmisOlayi` | `Business/Gecmis/GecmisModelleri.cs` | Tür (randevu/belirti), tarih, başlık, bölüm, doktor, durum/aciliyet, belirtiler, açıklama, doktor notu, yapay zekâ özeti |
| `GecmisOzeti` | aynı dosya | Toplam randevu, tamamlanan, iptal, gelmedi, belirti girişi sayısı, son muayene |
| `ErisimKaydi` (yeni tablo) | `ErisimKayitlari`, migration `Hafta11ErisimKaydi` | Hangi hastanın geçmişine, kim, hangi rolle, ne zaman erişti |
| `Randevu.DoktorNotu` | mevcut alan | Doktorun muayene notu (bu hafta arayüzden yazılabilir hale geldi) |

İlişkiler: Hasta 1–N ErisimKaydi (hasta silinirse izleri de silinir, CASCADE); Kullanıcı 1–N ErisimKaydi (izi olan kullanıcı silinemez, RESTRICT, çünkü iz kaybolmamalı).

## Erişim yetkisi (`HastaGecmisiService`)

| Rol | Kimin geçmişi | Ne görür |
|---|---|---|
| Hasta | Yalnızca kendisinin | Her şey + **"geçmişimi kimler görüntüledi"** listesi |
| Doktor | Yalnızca **en az bir randevusu olan** (tedavi ilişkisi bulunan) hastaların | Tıbbi içerik dahil her şey; kendi randevularına muayene notu yazabilir |
| Yönetici | Tüm hastaların | Randevu tarihleri, bölüm, doktor ve durum. **Tıbbi içerik gizli**: belirtiler, açıklamalar, doktor notları, yapay zekâ gerekçesi, yaş, cinsiyet |

- Kural **sunucuda** uygulanır; adres çubuğuna başka bir `hastaId` yazmak işe yaramaz.
- Yetkisiz istekte ve olmayan hastada **aynı** cevap verilir ("bulunamadı", HTTP 404). Böylece bir hastanın sistemde olup olmadığı bile sızmaz.
- Yöneticinin tıbbi içeriği görmemesi KVKK'daki **veri minimizasyonu** ilkesinden geliyor: yöneticinin işi randevu düzeni, muayene içeriği değil.

## Erişim izi
- Doktor ya da yönetici bir hastanın geçmişini her açtığında `ErisimKayitlari` tablosuna kayıt düşer.
- Hastanın kendi erişimi ve **yetkisiz denemeler** kaydedilmez, çünkü yetkisiz denemede veri gösterilmedi.
- Hasta kendi geçmiş sayfasında kimin ne zaman baktığını görür. Yönetici tüm izleri **Erişim kayıtları** sayfasında görür.

## Filtreleme

| Filtre | Kural |
|---|---|
| Tarih aralığı | Bitiş başlangıçtan önce olamaz; aralık en fazla 2 yıl; bitiş günü dahil |
| Tür | Randevular / belirti girişleri / tümü |
| Randevu durumu | Seçilirse yalnızca randevular listelenir. "Belirti" türüyle birlikte seçilemez |
| Bölüm | Randevuda doktorun bölümü, belirti kaydında önerilen bölüm |
| Sayfa | Sayfa başına 20 kayıt |

- Filtreler veritabanında (SQL `WHERE`) uygulanır ve mevcut indeksleri kullanır: `Randevular(HastaId, BaslangicZamani)`, `BelirtiKayitlari(HastaId, OlusturmaTarihi)`.
- Saat dilimi: randevular poliklinik yerel saatiyle, belirti kayıtları UTC ile tutuluyor. Tarih filtresi her biri için doğru saate çevrilerek uygulanır.
- Geçersiz filtrede hata mesajı gösterilir ve filtresiz liste açılır.
- Özet sayılar filtreden bağımsızdır ve tüm geçmişten hesaplanır.

## Muayene notu (iş kuralları)
- Notu yalnızca randevunun **kendi doktoru** yazabilir.
- Not, **randevu saati geldikten sonra** yazılabilir. Önceden not yazılamaz.
- İptal edilmiş randevuya not yazılamaz.
- En fazla 2000 karakter; boş not mevcut notu siler.

## Diğer modüllerle entegrasyon
| Modül | Bağlantı |
|---|---|
| Kimlik / roller (H5) | Erişim kararı oturumdaki rol ve hasta/doktor kimliğine göre verilir |
| Randevu (H6) | Randevular, durumları ve doktor notları geçmişin ana parçası; doktor takviminde hasta adından geçmişe gidilir |
| Belirti / yönlendirme (H7) | Belirti girişleri, aciliyet ve önerilen bölüm geçmişte görünür |
| Yapay zekâ (H10) | Başarılı yapay zekâ önerisi özet olarak görünür (yöneticide gizli) |
| Yönetim | Kullanıcılar listesinde hastalar için "Geçmiş" bağlantısı, ayrıca erişim kayıtları sayfası |

## Ekranlar
| Rol | Sayfa |
|---|---|
| Hasta | Menü → **Geçmişim** (`/Gecmis`) |
| Doktor | Menü → **Hastalarım** → Geçmiş; ya da Takvimim'de hasta adına tıklama |
| Yönetici | Kullanıcılar → hastanın yanındaki **Geçmiş**; menü → **Erişim kayıtları** |

## Testler (`tests/AkilliRandevu.Tests/Gecmis/HastaGecmisiTestleri.cs`)
Erişim kuralları (hasta, doktor ilişkili/ilişkisiz, yönetici), yöneticide tıbbi içeriğin gizlenmesi, erişim izi ve yetkisiz denemenin iz bırakmaması, filtre doğrulama, tür/durum/bölüm/tarih filtreleri, özet sayıları, doktorun hasta listesi, muayene notu kuralları.
