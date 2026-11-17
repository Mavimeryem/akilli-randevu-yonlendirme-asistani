# Ara Sürüm – Kısa Demo Senaryosu

Bu senaryo, ara sürümde tüm modüllerin uçtan uca çalıştığını göstermek için kullanılır.
Tahmini süre: **5–7 dakika**.

## Ön koşul

Uygulama `dotnet run --project src/AkilliRandevu.Web` ile başlatılmış ve
tarayıcıda `http://localhost:5027` adresi açık.

---

## Adım 1 – Hasta girişi (1 dk)

1. Ana sayfada **Giriş yap** → `hasta1@ornek.test` / `Demo1234` ile giriş.
2. Üst menüde Hasta'ya özel bağlantıların göründüğünü göster:
   *Belirti gir · Randevu al · Randevularım · Belirti kayıtlarım*

---

## Adım 2 – Belirti girişi ve yönlendirme (2 dk)

1. **Belirti gir** bağlantısına tıkla.
2. **Göğüs ağrısı** (şiddet 6) ve **Çarpıntı** (şiddet 6) seçilir.
3. **Değerlendir** butonuna bas.
4. Sonuç sayfasında:
   - Önerilen bölüm: **Kardiyoloji %64** (ilk öneride "Önerilen" rozeti).
     Hesap: Kardiyoloji 9×6 + 9×6 = 108, Göğüs Hastalıkları 6×6 = 36, Dahiliye 4×6 = 24 → 108 / 168 ≈ %64.
   - Uygunluk yüzdesi progress bar ile gösterilir.
   - Alt kısımda "Bu sonuç bir tıbbi tanı değildir…" uyarısı.
5. **Randevu al** butonuna bas → Randevu oluşturma sayfasına geç.

---

## Adım 3 – Randevu alma (1 dk)

1. Bölüm **Kardiyoloji** önceki adımdan seçili gelir.
2. Doktor olarak **Prof. Dr. Zeynep Çelik** (`doktor3`) seç, ileri tarihli boş bir slot seç.
3. Onay penceresinde **Tamam** → "… randevu talebiniz alındı. Doktor onayı bekleniyor." mesajı.
4. **Randevularım** sayfasında yeni randevunun *Beklemede* durumunda listelendiğini göster.

---

## Adım 4 – Doktor onayı (1 dk)

1. Çıkış yap, `doktor3@ornek.test` / `Demo1234` ile giriş (randevuyu alınan doktor).
2. **Takvimim** → hastanın randevusunu bul → **Onayla**.
3. Randevu durumu *Onaylandı* olur.

---

## Adım 5 – Yönetici görünümü (30 sn)

1. Çıkış yap, `yonetici@ornek.test` / `Demo1234` ile giriş.
2. **Randevular** sayfasında tüm randevuları, bölüm filtresiyle göster.

---

## Acil durum senaryosu (isteğe bağlı, 1 dk)

1. Hasta ile giriş yap → Belirti gir.
2. **Göğüs ağrısı** (şiddet 9) + **Nefes darlığı** (şiddet 8) seç.
3. Sonuç sayfasında kırmızı **ACİL** uyarısı ve 112 butonu gösterilir, randevu butonu gizlenir.

---

## Gösterilecek kontrol kriterleri

| Kriter | Nerede gösterilir |
|---|---|
| Kullanıcı + veritabanı çalışıyor | Giriş + listeleme ekranları |
| Modüller entegre | Belirti → Yönlendirme → Randevu akışı |
| Rol bazlı erişim | Hasta / Doktor / Yönetici menüleri |
| İş kuralları | Acil uyarısı, slot çakışması koruması |
