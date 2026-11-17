# ER Diyagramı

GitHub bu diyagramı otomatik olarak çizer (Mermaid).

```mermaid
erDiagram
    KULLANICI ||--o| HASTA : "1-1 (hasta ise)"
    KULLANICI ||--o| DOKTOR : "1-1 (doktor ise)"
    KULLANICI ||--o{ BILDIRIM : "1-N alir"
    BOLUM ||--o{ DOKTOR : "1-N calistirir"
    HASTA ||--o{ RANDEVU : "1-N alir"
    DOKTOR ||--o{ RANDEVU : "1-N verir"
    HASTA ||--o{ BELIRTI_KAYDI : "1-N girer"
    BOLUM |o--o{ BELIRTI_KAYDI : "onerilen bolum"
    BELIRTI_KAYDI |o--o| RANDEVU : "1-0..1 donusur"
    BELIRTI_KAYDI ||--|{ BELIRTI_KAYDI_BELIRTI : icerir
    BELIRTI ||--o{ BELIRTI_KAYDI_BELIRTI : "secilir"
    BELIRTI ||--o{ BELIRTI_BOLUM : "isaret eder"
    BOLUM ||--o{ BELIRTI_BOLUM : "ilgilenir"
    BILDIRIM ||--o{ EPOSTA_GONDERIMI : "1-N e-posta kanali (H9)"
    BELIRTI_KAYDI ||--o| YZ_DEGERLENDIRMESI : "1-0..1 yapay zeka (H10)"
    BOLUM |o--o{ YZ_DEGERLENDIRMESI : "YZ onerisi"
    HASTA ||--o{ ERISIM_KAYDI : "1-N erisim izi (H11)"
    KULLANICI ||--o{ ERISIM_KAYDI : "1-N eristi"

    KULLANICI {
        int Id PK
        string Ad
        string Soyad
        string Eposta UK
        string SifreHash
        string Telefon
        int Rol "Hasta/Doktor/Yonetici"
        bool AktifMi
        datetime OlusturmaTarihi
    }
    HASTA {
        int Id PK
        int KullaniciId FK,UK
        string TcKimlikNo UK
        date DogumTarihi
        int Cinsiyet
    }
    DOKTOR {
        int Id PK
        int KullaniciId FK,UK
        int BolumId FK
        string Unvan
    }
    BOLUM {
        int Id PK
        string Ad UK
        string Aciklama
        bool AktifMi
    }
    BELIRTI {
        int Id PK
        string Ad UK
        string Aciklama
    }
    BELIRTI_BOLUM {
        int BelirtiId PK,FK
        int BolumId PK,FK
        int AgirlikPuani
    }
    BELIRTI_KAYDI {
        int Id PK
        int HastaId FK
        int OnerilenBolumId FK "null olabilir"
        string Aciklama
        int Aciliyet
        datetime OlusturmaTarihi
    }
    BELIRTI_KAYDI_BELIRTI {
        int BelirtiKaydiId PK,FK
        int BelirtiId PK,FK
        int Siddet "1-10"
    }
    RANDEVU {
        int Id PK
        int HastaId FK
        int DoktorId FK
        int BelirtiKaydiId FK,UK "null olabilir"
        datetime BaslangicZamani
        datetime BitisZamani
        int Durum
        string DoktorNotu
        datetime HatirlatmaTarihi "H9, null olabilir"
    }
    BILDIRIM {
        int Id PK
        int KullaniciId FK
        string Baslik
        string Mesaj
        bool OkunduMu
    }
    EPOSTA_GONDERIMI {
        int Id PK
        int BildirimId FK
        string Alici
        int Durum "Bekliyor/Gonderildi/Basarisiz/Atlandi"
        int DenemeSayisi
        string SonHata
        string DisMesajId
        datetime GonderimTarihi
    }
    YZ_DEGERLENDIRMESI {
        int Id PK
        int BelirtiKaydiId FK,UK
        int Durum "Basarili/Hatali/Gecersiz/Reddedildi"
        int OnerilenBolumId FK "null olabilir"
        string Gerekce
        int Guven "Dusuk/Orta/Yuksek"
        bool AcilBelirtiVar
        string AcilGerekce
        string Model
        string IstemSurumu
        int SureMs
        string Hata
    }
    ERISIM_KAYDI {
        int Id PK
        int HastaId FK
        int ErisenKullaniciId FK
        int ErisenRol
        string Islem
        datetime OlusturmaTarihi
    }
```

## İlişki özeti

| İlişki | Tür | Nasıl kuruldu |
|---|---|---|
| Kullanici – Hasta | 1-1 | `Hasta.KullaniciId` FK + benzersiz |
| Kullanici – Doktor | 1-1 | `Doktor.KullaniciId` FK + benzersiz |
| Kullanici – Bildirim | 1-N | `Bildirim.KullaniciId` FK |
| Bolum – Doktor | 1-N | `Doktor.BolumId` FK |
| Hasta – Randevu | 1-N | `Randevu.HastaId` FK |
| Doktor – Randevu | 1-N | `Randevu.DoktorId` FK |
| Hasta – BelirtiKaydi | 1-N | `BelirtiKaydi.HastaId` FK |
| BelirtiKaydi – Randevu | 1-0..1 | `Randevu.BelirtiKaydiId` FK (null olabilir, benzersiz) |
| Belirti – Bolum | N-N | `BelirtiBolum` ara tablosu, birleşik PK |
| BelirtiKaydi – Belirti | N-N | `BelirtiKaydiBelirti` ara tablosu, birleşik PK |
| Bildirim – EpostaGonderimi (H9) | 1-N | `EpostaGonderimi.BildirimId` FK, CASCADE |
| BelirtiKaydi – YzDegerlendirmesi (H10) | 1-0..1 | `YzDegerlendirmesi.BelirtiKaydiId` FK + benzersiz, CASCADE |
| Bolum – YzDegerlendirmesi (H10) | 1-N (opsiyonel) | `YzDegerlendirmesi.OnerilenBolumId` FK, SET NULL |
| Hasta – ErisimKaydi (H11) | 1-N | `ErisimKaydi.HastaId` FK, CASCADE |
| Kullanici – ErisimKaydi (H11) | 1-N | `ErisimKaydi.ErisenKullaniciId` FK, RESTRICT (iz silinmesin) |

## Normalizasyon notları

- **1NF:** Her alan tek değer tutar. Bir kayıttaki birden fazla belirti virgülle tek sütuna yazılmaz, `BelirtiKaydiBelirti` tablosunda ayrı satırlar olarak tutulur.
- **2NF:** Birleşik anahtarlı tablolarda (`BelirtiBolum`, `BelirtiKaydiBelirti`) anahtar dışı alanlar (`AgirlikPuani`, `Siddet`) anahtarın tamamına bağlıdır, bir parçasına değil.
- **3NF:** Geçişli bağımlılık yoktur. Örneğin randevunun bölümü `Randevu` tablosunda tutulmaz, çünkü bölüm doktordan belirlenir (Randevu → Doktor → Bolum). Hastanın adı ve soyadı `Hasta` tablosunda tekrar edilmez, `Kullanici` tablosundan gelir.
