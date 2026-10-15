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
    }
    BILDIRIM {
        int Id PK
        int KullaniciId FK
        string Baslik
        string Mesaj
        bool OkunduMu
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

## Normalizasyon notları

- **1NF:** Her alan tek değer tutar. Bir kayıttaki birden fazla belirti virgülle tek sütuna yazılmaz, `BelirtiKaydiBelirti` tablosunda ayrı satırlar olarak tutulur.
- **2NF:** Birleşik anahtarlı tablolarda (`BelirtiBolum`, `BelirtiKaydiBelirti`) anahtar dışı alanlar (`AgirlikPuani`, `Siddet`) anahtarın tamamına bağlıdır, bir parçasına değil.
- **3NF:** Geçişli bağımlılık yoktur. Örneğin randevunun bölümü `Randevu` tablosunda tutulmaz, çünkü bölüm doktordan belirlenir (Randevu → Doktor → Bolum). Hastanın adı ve soyadı `Hasta` tablosunda tekrar edilmez, `Kullanici` tablosundan gelir.
