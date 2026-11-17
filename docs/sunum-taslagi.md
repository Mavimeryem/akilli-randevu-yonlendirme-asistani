# Sunum Taslağı

Bu dosya yalnızca bir **anahat**tır; slaytlar öğrenci tarafından kendi cümleleriyle hazırlanır. Önerilen süre 10–12 dakika, 10–12 slayt.

| # | Slayt | İçerik önerisi |
|---|---|---|
| 1 | Başlık | Proje adı, P06, ad soyad, numara, ders |
| 2 | Problem | Hasta hangi polikliniğe gideceğini bilmiyor; yanlış bölüm → zaman kaybı ve gereksiz yoğunluk |
| 3 | Çözüm ve kapsam | Belirti → bölüm önerisi → randevu; 3 rol; sınır: tanı koymaz |
| 4 | Mimari | `mimari.md` şeması; neden katmanlı yapı |
| 5 | Veri modeli | ER diyagramı; 3NF örneği (randevunun bölümü doktordan gelir) |
| 6 | Yönlendirme | Ağırlıklı puan formülü ve örnek hesap (108/168 = %64); acil kuralları |
| 7 | Yapay zekâ | Neden ikinci görüş olarak kullanıldı; JSON şeması, doğrulama, maskeleme, onay, geri dönüş |
| 8 | Dış API ve bildirim | Giden kutusu deseni, tekrar deneme; anahtarlar ortam değişkeninde |
| 9 | Güvenlik ve KVKK | PBKDF2, rol + sahiplik kontrolü, erişim izi, maskeli sıra panosu, CSV'de ad yok |
| 10 | Test | 164+ test, kapsam, CI; H-03 hatasının hikâyesi |
| 11 | Canlı demo | `final-demo-senaryosu.md` |
| 12 | Sonuç | Öğrendiklerim, sınırlar, gelecek çalışmalar (kalıcı veritabanı, SMS, mobil uyum, klinik doğrulama) |

## Kendine sorman gereken sorular
- Hangi kararı ben verdim, hangisini yapay zekâ önerdi ve neden kabul ettim?
- Yapay zekâ çıktısı yanlış olursa sistem ne yapıyor?
- Bulutta saat neden kaydı ve nasıl buldum?
