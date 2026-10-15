-- =====================================================================
-- Akıllı Randevu - Elle yazılmış SQL örnekleri (SQLite)
-- Tabloların asıl kaynağı EF Core migration'larıdır
-- (src/AkilliRandevu.DataAccess/Migrations). Bu dosya, ilişki/kısıt ve
-- CRUD işlemlerinin SQL karşılığını göstermek için elle yazılmıştır.
-- =====================================================================


-- ---------------------------------------------------------------------
-- 1) Elle yazılan ilişki ve kısıtlar: Randevular tablosu
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Randevular" (
    "Id"              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,   -- birincil anahtar
    "HastaId"         INTEGER NOT NULL,
    "DoktorId"        INTEGER NOT NULL,
    "BelirtiKaydiId"  INTEGER NULL,
    "BaslangicZamani" TEXT    NOT NULL,
    "BitisZamani"     TEXT    NOT NULL,
    "Durum"           INTEGER NOT NULL DEFAULT 1,
    "DoktorNotu"      TEXT    NULL,
    "OlusturmaTarihi" TEXT    NOT NULL,

    -- CHECK kısıtı: bitiş başlangıçtan sonra olmalı, durum 1-5 arası olmalı
    CONSTRAINT "CK_Randevu_Zaman"  CHECK ("BitisZamani" > "BaslangicZamani"),
    CONSTRAINT "CK_Randevu_Durum"  CHECK ("Durum" BETWEEN 1 AND 5),

    -- 1-N ilişki: bir hastanın çok randevusu olur.
    -- RESTRICT: randevusu olan hasta silinemez (geçmiş kaybolmasın).
    CONSTRAINT "FK_Randevular_Hastalar_HastaId"
        FOREIGN KEY ("HastaId") REFERENCES "Hastalar" ("Id") ON DELETE RESTRICT,

    -- 1-N ilişki: bir doktorun çok randevusu olur.
    CONSTRAINT "FK_Randevular_Doktorlar_DoktorId"
        FOREIGN KEY ("DoktorId") REFERENCES "Doktorlar" ("Id") ON DELETE RESTRICT,

    -- 1-0..1 ilişki: randevu bir belirti kaydından doğmuş olabilir.
    -- SET NULL: belirti kaydı silinirse randevu kalır, bağlantı boşaltılır.
    CONSTRAINT "FK_Randevular_BelirtiKayitlari_BelirtiKaydiId"
        FOREIGN KEY ("BelirtiKaydiId") REFERENCES "BelirtiKayitlari" ("Id") ON DELETE SET NULL
);

-- İndeksler: sık yapılan sorguları hızlandırır.
CREATE INDEX IF NOT EXISTS "IX_Randevular_DoktorId_BaslangicZamani" ON "Randevular" ("DoktorId", "BaslangicZamani");
CREATE INDEX IF NOT EXISTS "IX_Randevular_HastaId_BaslangicZamani"  ON "Randevular" ("HastaId", "BaslangicZamani");
-- Benzersiz indeks: bir belirti kaydı en fazla bir randevuya bağlanır (1-0..1).
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Randevular_BelirtiKaydiId" ON "Randevular" ("BelirtiKaydiId");


-- ---------------------------------------------------------------------
-- 2) N-N ilişki: Belirti <-> Bolum (ara tablo, birleşik birincil anahtar)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "BelirtiBolumleri" (
    "BelirtiId"    INTEGER NOT NULL,
    "BolumId"      INTEGER NOT NULL,
    "AgirlikPuani" INTEGER NOT NULL,
    CONSTRAINT "PK_BelirtiBolumleri" PRIMARY KEY ("BelirtiId", "BolumId"),
    CONSTRAINT "CK_BelirtiBolum_AgirlikPuani" CHECK ("AgirlikPuani" BETWEEN 1 AND 10),
    CONSTRAINT "FK_BelirtiBolumleri_Belirtiler_BelirtiId" FOREIGN KEY ("BelirtiId") REFERENCES "Belirtiler" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BelirtiBolumleri_Bolumler_BolumId"     FOREIGN KEY ("BolumId")   REFERENCES "Bolumler" ("Id")   ON DELETE CASCADE
);


-- ---------------------------------------------------------------------
-- 3) CRUD örnekleri (Bolumler tablosu)
-- ---------------------------------------------------------------------
-- CREATE
INSERT INTO "Bolumler" ("Ad", "Aciklama", "AktifMi", "OlusturmaTarihi")
VALUES ('Üroloji', 'Böbrek ve idrar yolları', 1, '2026-10-15 10:00:00');

-- READ
SELECT "Id", "Ad", "AktifMi" FROM "Bolumler" WHERE "AktifMi" = 1 ORDER BY "Ad";

-- UPDATE
UPDATE "Bolumler" SET "Aciklama" = 'Böbrek, mesane ve idrar yolları' WHERE "Ad" = 'Üroloji';

-- DELETE
DELETE FROM "Bolumler" WHERE "Ad" = 'Üroloji';


-- ---------------------------------------------------------------------
-- 4) Modüllere ait temel sorgular
-- ---------------------------------------------------------------------
-- Hasta geçmişi: 1 numaralı hastanın randevuları (doktor ve bölümüyle)
SELECT r."BaslangicZamani", b."Ad" AS "Bolum",
       d."Unvan" || ' ' || k."Ad" || ' ' || k."Soyad" AS "Doktor", r."Durum"
FROM "Randevular" r
JOIN "Doktorlar"     d ON d."Id" = r."DoktorId"
JOIN "Kullanicilar"  k ON k."Id" = d."KullaniciId"
JOIN "Bolumler"      b ON b."Id" = d."BolumId"
WHERE r."HastaId" = 1
ORDER BY r."BaslangicZamani" DESC;

-- Bölüm yönlendirme: "Baş ağrısı" (1) + "Baş dönmesi" (2) seçildiyse bölüm puanları
SELECT b."Id", b."Ad", SUM(bb."AgirlikPuani") AS "ToplamPuan"
FROM "BelirtiBolumleri" bb
JOIN "Bolumler" b ON b."Id" = bb."BolumId"
WHERE bb."BelirtiId" IN (1, 2) AND b."AktifMi" = 1
GROUP BY b."Id", b."Ad"
ORDER BY "ToplamPuan" DESC;

-- Randevu çakışma kontrolü: 5 numaralı doktor 05.10.2026 09:10-09:30 arası dolu mu?
SELECT COUNT(*) > 0 AS "CakismaVar"
FROM "Randevular"
WHERE "DoktorId" = 5
  AND "Durum" <> 4                                  -- iptal edilenler hariç
  AND "BaslangicZamani" < '2026-10-05 09:30:00'
  AND "BitisZamani"     > '2026-10-05 09:10:00';

-- Okunmamış bildirimler
SELECT "Baslik", "Mesaj" FROM "Bildirimler" WHERE "KullaniciId" = 12 AND "OkunduMu" = 0;


-- ---------------------------------------------------------------------
-- 5) İşlem (transaction): randevu iptali + bildirim birlikte ya hep ya hiç
-- ---------------------------------------------------------------------
BEGIN TRANSACTION;
UPDATE "Randevular" SET "Durum" = 4 WHERE "Id" = 9;
INSERT INTO "Bildirimler" ("KullaniciId", "Baslik", "Mesaj", "OkunduMu", "OlusturmaTarihi")
VALUES (18, 'Randevu iptal edildi', '22.10.2026 15:00 Göz randevunuz iptal edildi.', 0, '2026-10-15 10:05:00');
COMMIT;
