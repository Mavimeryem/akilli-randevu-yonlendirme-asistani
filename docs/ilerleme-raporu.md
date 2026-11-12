# Kısa İlerleme Raporu – Ara Sürüm (Hafta 8)

## Özet

Proje, planlandığı şekilde 7 haftada temel modülleri tamamladı ve bu haftada
ara sürüm (v0.8.0) olarak etiketlendi. Tüm modüller uçtan uca entegre çalışmaktadır.

## Tamamlanan Modüller

| Modül | Durum | Notlar |
|---|---|---|
| Katmanlı mimari ve veri modeli | ✅ | Hafta 3 |
| Veritabanı ve EF Core | ✅ | Hafta 4; SQLite, migration, seed veri |
| Kimlik doğrulama ve roller | ✅ | Hafta 5; cookie auth, BCrypt, 3 rol |
| Randevu takvimi | ✅ | Hafta 6; slot üretici, durum makinesi, çakışma koruması |
| Belirti girişi ve yönlendirme | ✅ | Hafta 7; kural motoru, aciliyet, bölüm önerisi |
| Ara sürüm entegrasyonu | ✅ | Hafta 8; uçtan uca test, hata giderme, etiket |

## Entegrasyon Akışı

```
Hasta → Giriş Yap → Belirti Gir → Yönlendirme Sonucu → Randevu Al
                                        ↓
                                   Acil Uyarısı (112)
                                        ↓
                              Doktor Onayı / Yönetici Görünümü
```

Akışın tüm adımları `hasta1@ornek.test` ile test edilmiş ve doğru çalıştığı
görülmüştür.

## Bilinen Sınırlar

- Belirti yönlendirme kuralları kural tabanlı ve basittir; makine öğrenmesi yoktur.
- Bildirimler yalnızca veritabanına yazılmaktadır; e-posta veya push bildirimi yoktur.
- Raporlama modülü henüz eklenmemiştir (Hafta 9+).

## Sonraki Adımlar (Hafta 9+)

- Hasta geçmişi ve raporlama ekranları.
- Bildirim sistemi (e-posta entegrasyonu).
- Yönetici paneli genişletmesi (bölüm ve doktor yönetimi).
- Performans ve güvenlik iyileştirmeleri.

## Git Etiketi

Ara sürüm `v0.8.0` etiketiyle işaretlenmiştir:

```bash
git tag -a v0.8.0 -m "Ara sürüm: Hafta 3–7 modülleri entegre, uçtan uca çalışıyor"
```
