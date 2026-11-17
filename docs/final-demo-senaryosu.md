# Final Demo Senaryosu (savunma)

Süre: **~10 dakika**. Bütün hesapların şifresi `Demo1234`. Canlı adres yoksa: `docker run -p 8080:8080 -e DemoModu=true akilli-randevu`.

| Dk | Rol | Adım | Gösterilen |
|---|---|---|---|
| 0–1 | – | Ana sayfa, sayfa altındaki demo uyarısı, Gizlilik ve KVKK sayfası | Proje amacı, KVKK farkındalığı |
| 1–3 | Hasta (`hasta1`) | **Belirti gir:** Göğüs ağrısı 6 + Çarpıntı 6, açıklama yaz, yapay zekâ onayını işaretle → Değerlendir | Kural tabanlı öneri (**Kardiyoloji %64**) + yapay zekâ kutusu (bölüm, gerekçe, güven, sınır uyarısı) |
| 3–4 | Hasta | **Randevu al:** Kardiyoloji → `doktor3` → boş saat | Slot üretimi, çakışma koruması, "Beklemede" |
| 4 | Hasta | Menüdeki **Bildirimler** (okunmamış sayısı); **Geçmişim** → filtre (tür: Randevu) | Bildirim, geçmiş ve filtreleme |
| 4–5 | Hasta | (İsteğe bağlı) Göğüs ağrısı 9 + Nefes darlığı 8 | **ACİL** uyarısı, randevu butonu gizli |
| 5–6 | Doktor (`doktor3`) | **Takvimim** → randevuyu **Onayla**; **Hastalarım** → hastanın geçmişi → muayene notu | Durum kuralları, ilişkiye dayalı erişim |
| 6–7 | Doktor | **Sıra panosu** | Maskeli adlar (ör. `S*** K***`), sıra ve bekleme |
| 7–9 | Yönetici (`yonetici`) | **İstatistik** → tarih aralığı → grafikler → **CSV indir** | Yoğunluk, doluluk, iptal/gelmeme, ısı haritası, tablo görünümü |
| 9 | Yönetici | **Yönetim → Erişim kayıtları**, **E-posta gönderimleri** (→ test e-postası) | Erişim izi, dış API kayıtları |
| 9–10 | – | GitHub: Actions'ta yeşil CI, test sayısı ve kapsam | Kalite güvencesi |

## Olası sorular için hazır ekranlar
- `docs/mimari.md` – katman şeması
- `docs/ER-diyagrami.md` – veri modeli
- `docs/test-raporu.md` – test piramidi ve kapsam
- `docs/hata-listesi.md` – H-03 ayıklama adımları
- `docs/yapay-zeka-bileseni.md` – istem, şema, doğrulama ve sınırlar

## Bir şey ters giderse
| Durum | Ne söylenir / yapılır |
|---|---|
| Yapay zekâ kutusu "kullanılamıyor" | "Dış servis yanıt vermediğinde sistem kural tabanlı öneriyle devam ediyor; bu bilinçli bir tasarım." Hata kaydını göster. |
| Canlı adres yavaş | Ücretsiz planın uyku modu; yerel Docker'a geç. |
| E-posta gelmedi | Yönetim → E-posta gönderimleri: durum ve son hata görünüyor; uygulama içi bildirim yine var. |
