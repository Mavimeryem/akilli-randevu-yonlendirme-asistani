# Test Sonuç Raporu (Hafta 13)

## Özet
| | |
|---|---|
| Toplam test | **164** |
| Geçen / kalan | **164 / 0** |
| Satır kapsamı | **%89,7** |
| Dal kapsamı | **%78,1** |
| Ortam | GitHub Actions, `ubuntu-latest`, .NET 10, Release derlemesi |
| Kaynak | CI çalışması [44133530766](https://github.com/Mavimeryem/akilli-randevu-yonlendirme-asistani/actions/runs/44133530766) |

Kapsamdan otomatik üretilen kod çıkarıldı: EF Core migration dosyaları ve Razor'ın ürettiği sınıflar. Ayrıntılı HTML kapsam raporu her CI çalışmasının **test-sonuclari** eserinde (`kapsam/index.html`) bulunur.

## Test piramidi
```
          ┌──────────────────────────┐
          │  Web / uçtan uca  (43)   │  gerçek HTTP: sayfalar, yetki, tam akış
        ┌─┴──────────────────────────┴─┐
        │  Bileşen / veritabanı (50)   │  gerçek SQLite + servisler + migration
      ┌─┴──────────────────────────────┴─┐
      │  Birim (71)                      │  saf mantık, veritabanı ve ağ yok
      └──────────────────────────────────┘
```
| Katman | Test sayısı | Ne doğrular |
|---|---|---|
| **Birim** | 71 | Slot üretici, randevu durum kuralları, yönlendirme motoru, parola hash, giriş kilidi, poliklinik saati, e-posta API istemcisi (sahte HTTP sunucusu), yapay zekâ çıktı doğrulama ve maskeleme, istatistik hesapları, sıra panosu, CSV |
| **Bileşen** | 50 | Bellekte SQLite, **migration'larla** kurulur: randevu oluşturma sınırları, kayıt ve giriş, hatırlatma ve giden kutusu, hasta geçmişi erişim kuralları ve filtreler, yapay zekâ servis entegrasyonu, örnek veriyle istatistik, migration ve model uyumu |
| **Web / uçtan uca** | 43 | `WebApplicationFactory` ile uygulamanın tamamı: her rolün her sayfası açılıyor mu, yetkisiz erişim, anti-forgery, CSV indirme, kayıt → giriş, giriş kilidi, belirti → randevu → onay → bildirim → geçmiş |

### Dosyalara göre
| Dosya | Test |
|---|---|
| `Kurallar/KritikKurallarTestleri.cs` | 24 |
| `Kurallar/SinirDurumlariTestleri.cs` | 15 |
| `Entegrasyon/SayfaTestleri.cs` | 40 |
| `Entegrasyon/UctanUcaAkisTestleri.cs` | 3 |
| `Gecmis/HastaGecmisiTestleri.cs` | 18 |
| `Istatistik/IstatistikTestleri.cs` | 16 |
| `YapayZeka/YzCiktiDogrulayiciTestleri.cs` | 12 |
| `YapayZeka/BolumOneriAsistaniTestleri.cs` | 7 |
| `YapayZeka/KisiselVeriMaskeleyiciTestleri.cs` | 7 |
| `YapayZeka/BelirtiServisiYapayZekaTestleri.cs` | 6 |
| `Bildirim/BrevoEpostaGondericiTestleri.cs` | 7 |
| `Bildirim/HatirlatmaVeKuyrukTestleri.cs` | 7 |
| `Veritabani/MigrationTestleri.cs` | 2 |

## Hangi senaryolar neden test edildi?
| Senaryo | Neden |
|---|---|
| Çakışma: aynı slot, aynı saatte iki bölüm | Projenin en kritik iş kuralı; hatası doğrudan hasta mağduriyeti doğurur |
| Durum makinesi ve sahiplik | Başka hastanın randevusunu iptal etmek gibi yetki hataları güvenlik açığıdır |
| Acil durum kuralları ve sınırları (göğüs ağrısı 6 / 7) | Yanlış "acil değil" sonucu tehlikelidir; sınır değerleri ayrıca denendi |
| Yapay zekâ çıktısı: boş, bozuk, kesik, uydurma bölüm, ret, servis hatası | Dış servis her zaman doğru cevap vermez; uygulama çökmemeli ve kurallara dönmeli |
| E-posta API: 429, 5xx, 400, 401, zaman aşımı | Harici servis hataları gerçek hayatta mutlaka yaşanır |
| Erişim izi ve yöneticide tıbbi içeriğin gizlenmesi | KVKK |
| Her sayfa her rol için açılıyor mu | Razor görünümleri çalışma anında derlenir; bir görünüm hatası derleme sırasında yakalanmaz |
| Tam akış | Modüllerin birlikte çalıştığını tek bir senaryoda doğrular |

## Sınır durumu (edge case) örnekleri
| Durum | Beklenen | Test |
|---|---|---|
| Son slot 16:40 / mesai bitişi 17:00 | 16:40 alınır, 17:00 reddedilir | `Son_slot_alinabilir_mesai_bitisi_alinamaz` |
| 30. gün / 31. gün | 30. gün alınır, 31. gün reddedilir | `Otuzuncu_gun_alinabilir_otuz_birinci_gun_alinamaz` |
| Hasta iptali: tam 2 saat / 1 saat 59 dakika kala | İlki kabul, ikincisi ret | `Hasta_tam_iki_saat_kala_iptal_edebilir_...` |
| Şifre: 7 karakter / sadece harf / sadece rakam / tam 8 karakter (harf + rakam) | Yalnızca sonuncusu geçer | `Sifre_kurali_sinirlari` |
| TC: 10 hane / 12 hane / içinde harf | Reddedilir | `Gecersiz_TC_reddedilir` |
| 4. ve 5. hatalı giriş, 15 dk + 1 sn sonra | 5.'de kilit, süre dolunca açılır | `Dort_hata_kilitlemez_besinci_kilitler_...` |
| Payda sıfır (hafta sonu kapasitesi) | Oran "—", %0 değil | `Payda_sifirsa_oran_hesaplanmaz` |
| İki bölüm başa baş (%53 / %47) | Belirsiz → Dahiliye | `Iki_bolum_basa_bas_ise_sonuc_belirsizdir` |

## Neden %100 kapsam hedeflenmedi?
- Kapsam, kodun **çalıştırıldığını** gösterir; **doğru çalıştığını** göstermez. %100'e ulaşmak için anlamsız testler yazmak ölçüyü şişirir.
- Kalan kısmın çoğu, dış servisin gerçekten çağrıldığı satırlar (Claude SDK çağrısı, gerçek HTTP), geliştirme ortamına özel başlangıç kodu ve görünüm kodu. Bunlar sahte istemcilerle ya da web testleriyle dolaylı olarak doğrulanıyor.
- Hedef: kritik iş kurallarının **hepsinin** test edilmesi. Bu hedefe ulaşıldı.

## Testler nasıl çalıştırılır?
```bash
dotnet test                                    # tümü
dotnet test --filter "FullyQualifiedName~Kurallar"     # yalnızca kurallar
dotnet test --collect:"XPlat Code Coverage"    # kapsam ölçümüyle
```
GitHub Actions her push'ta derleme, migration/model uyumu, testler ve kapsam raporunu otomatik çalıştırır.
