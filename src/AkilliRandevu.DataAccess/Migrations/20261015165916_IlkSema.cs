using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AkilliRandevu.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class IlkSema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Belirtiler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Belirtiler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bolumler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bolumler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Eposta = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SifreHash = table.Column<string>(type: "TEXT", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Rol = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                    table.CheckConstraint("CK_Kullanici_Rol", "Rol IN (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "BelirtiBolumleri",
                columns: table => new
                {
                    BelirtiId = table.Column<int>(type: "INTEGER", nullable: false),
                    BolumId = table.Column<int>(type: "INTEGER", nullable: false),
                    AgirlikPuani = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BelirtiBolumleri", x => new { x.BelirtiId, x.BolumId });
                    table.CheckConstraint("CK_BelirtiBolum_AgirlikPuani", "AgirlikPuani BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "FK_BelirtiBolumleri_Belirtiler_BelirtiId",
                        column: x => x.BelirtiId,
                        principalTable: "Belirtiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BelirtiBolumleri_Bolumler_BolumId",
                        column: x => x.BolumId,
                        principalTable: "Bolumler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bildirimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: false),
                    Baslik = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Mesaj = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    OkunduMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bildirimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bildirimler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doktorlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: false),
                    BolumId = table.Column<int>(type: "INTEGER", nullable: false),
                    Unvan = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doktorlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doktorlar_Bolumler_BolumId",
                        column: x => x.BolumId,
                        principalTable: "Bolumler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doktorlar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hastalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: false),
                    TcKimlikNo = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 11, nullable: false),
                    DogumTarihi = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Cinsiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hastalar", x => x.Id);
                    table.CheckConstraint("CK_Hasta_TcKimlikNo", "length(TcKimlikNo) = 11");
                    table.ForeignKey(
                        name: "FK_Hastalar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BelirtiKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HastaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OnerilenBolumId = table.Column<int>(type: "INTEGER", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Aciliyet = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BelirtiKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BelirtiKayitlari_Bolumler_OnerilenBolumId",
                        column: x => x.OnerilenBolumId,
                        principalTable: "Bolumler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BelirtiKayitlari_Hastalar_HastaId",
                        column: x => x.HastaId,
                        principalTable: "Hastalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BelirtiKaydiBelirtileri",
                columns: table => new
                {
                    BelirtiKaydiId = table.Column<int>(type: "INTEGER", nullable: false),
                    BelirtiId = table.Column<int>(type: "INTEGER", nullable: false),
                    Siddet = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BelirtiKaydiBelirtileri", x => new { x.BelirtiKaydiId, x.BelirtiId });
                    table.CheckConstraint("CK_BelirtiKaydiBelirti_Siddet", "Siddet BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "FK_BelirtiKaydiBelirtileri_BelirtiKayitlari_BelirtiKaydiId",
                        column: x => x.BelirtiKaydiId,
                        principalTable: "BelirtiKayitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BelirtiKaydiBelirtileri_Belirtiler_BelirtiId",
                        column: x => x.BelirtiId,
                        principalTable: "Belirtiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HastaId = table.Column<int>(type: "INTEGER", nullable: false),
                    DoktorId = table.Column<int>(type: "INTEGER", nullable: false),
                    BelirtiKaydiId = table.Column<int>(type: "INTEGER", nullable: true),
                    BaslangicZamani = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisZamani = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    DoktorNotu = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.Id);
                    table.CheckConstraint("CK_Randevu_Durum", "Durum BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_Randevu_Zaman", "BitisZamani > BaslangicZamani");
                    table.ForeignKey(
                        name: "FK_Randevular_BelirtiKayitlari_BelirtiKaydiId",
                        column: x => x.BelirtiKaydiId,
                        principalTable: "BelirtiKayitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Randevular_Doktorlar_DoktorId",
                        column: x => x.DoktorId,
                        principalTable: "Doktorlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Randevular_Hastalar_HastaId",
                        column: x => x.HastaId,
                        principalTable: "Hastalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Belirtiler",
                columns: new[] { "Id", "Aciklama", "Ad", "OlusturmaTarihi" },
                values: new object[,]
                {
                    { 1, null, "Baş ağrısı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, null, "Baş dönmesi", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, null, "Göğüs ağrısı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, null, "Çarpıntı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, null, "Nefes darlığı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, null, "Öksürük", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, null, "Ateş", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, null, "Karın ağrısı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, null, "Bulantı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, null, "Boğaz ağrısı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, null, "Kulak ağrısı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, null, "Bulanık görme", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, null, "Eklem ağrısı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, null, "Bel ağrısı", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, null, "Ciltte döküntü", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, null, "El ve ayaklarda uyuşma", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Bolumler",
                columns: new[] { "Id", "Aciklama", "Ad", "AktifMi", "OlusturmaTarihi" },
                values: new object[,]
                {
                    { 1, null, "Dahiliye", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, null, "Kardiyoloji", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, null, "Nöroloji", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, null, "Kulak Burun Boğaz", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, null, "Göz Hastalıkları", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, null, "Ortopedi", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, null, "Dermatoloji", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, null, "Göğüs Hastalıkları", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "Id", "Ad", "AktifMi", "Eposta", "OlusturmaTarihi", "Rol", "SifreHash", "Soyad", "Telefon" },
                values: new object[,]
                {
                    { 1, "Sistem", true, "yonetici@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 3, "SENTETIK-HASH", "Yöneticisi", "0555 000 0001" },
                    { 2, "Ayşe", true, "doktor1@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Kaya", "0555 000 0002" },
                    { 3, "Mehmet", true, "doktor2@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Demir", "0555 000 0003" },
                    { 4, "Zeynep", true, "doktor3@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Çelik", "0555 000 0004" },
                    { 5, "Ali", true, "doktor4@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Şahin", "0555 000 0005" },
                    { 6, "Elif", true, "doktor5@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Yıldız", "0555 000 0006" },
                    { 7, "Mustafa", true, "doktor6@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Aydın", "0555 000 0007" },
                    { 8, "Fatma", true, "doktor7@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Öztürk", "0555 000 0008" },
                    { 9, "Hasan", true, "doktor8@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Arslan", "0555 000 0009" },
                    { 10, "Merve", true, "doktor9@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Doğan", "0555 000 0010" },
                    { 11, "Emre", true, "doktor10@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "SENTETIK-HASH", "Kılıç", "0555 000 0011" },
                    { 12, "Deniz", true, "hasta1@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "SENTETIK-HASH", "Acar", "0555 000 0012" },
                    { 13, "Burak", true, "hasta2@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "SENTETIK-HASH", "Koç", "0555 000 0013" },
                    { 14, "Selin", true, "hasta3@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "SENTETIK-HASH", "Kurt", "0555 000 0014" },
                    { 15, "Can", true, "hasta4@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "SENTETIK-HASH", "Özdemir", "0555 000 0015" },
                    { 16, "Ece", true, "hasta5@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "SENTETIK-HASH", "Polat", "0555 000 0016" },
                    { 17, "Kerem", true, "hasta6@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "SENTETIK-HASH", "Aslan", "0555 000 0017" },
                    { 18, "Gizem", true, "hasta7@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "SENTETIK-HASH", "Tekin", "0555 000 0018" },
                    { 19, "Oğuz", true, "hasta8@ornek.test", new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "SENTETIK-HASH", "Yavuz", "0555 000 0019" }
                });

            migrationBuilder.InsertData(
                table: "BelirtiBolumleri",
                columns: new[] { "BelirtiId", "BolumId", "AgirlikPuani" },
                values: new object[,]
                {
                    { 1, 1, 4 },
                    { 1, 3, 8 },
                    { 1, 5, 3 },
                    { 2, 2, 4 },
                    { 2, 3, 7 },
                    { 2, 4, 6 },
                    { 3, 2, 9 },
                    { 3, 8, 6 },
                    { 4, 1, 4 },
                    { 4, 2, 9 },
                    { 5, 2, 7 },
                    { 5, 8, 8 },
                    { 6, 1, 3 },
                    { 6, 4, 4 },
                    { 6, 8, 8 },
                    { 7, 1, 7 },
                    { 7, 4, 4 },
                    { 8, 1, 9 },
                    { 9, 1, 7 },
                    { 9, 3, 3 },
                    { 10, 1, 4 },
                    { 10, 4, 9 },
                    { 11, 4, 10 },
                    { 12, 3, 5 },
                    { 12, 5, 10 },
                    { 13, 1, 4 },
                    { 13, 6, 8 },
                    { 14, 3, 4 },
                    { 14, 6, 9 },
                    { 15, 1, 3 },
                    { 15, 7, 10 },
                    { 16, 3, 9 },
                    { 16, 6, 5 }
                });

            migrationBuilder.InsertData(
                table: "Bildirimler",
                columns: new[] { "Id", "Baslik", "KullaniciId", "Mesaj", "OkunduMu", "OlusturmaTarihi" },
                values: new object[,]
                {
                    { 1, "Randevu hatırlatma", 12, "20.10.2026 09:00 Dahiliye randevunuz var.", false, new DateTime(2026, 10, 19, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Randevu onaylandı", 16, "20.10.2026 13:00 Dermatoloji randevunuz onaylandı.", true, new DateTime(2026, 10, 12, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Randevu iptal edildi", 17, "13.10.2026 09:40 Dahiliye randevunuz iptal edildi.", true, new DateTime(2026, 10, 13, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "Randevu onaylandı", 13, "21.10.2026 10:20 Kardiyoloji randevunuz onaylandı.", false, new DateTime(2026, 10, 15, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "Yeni randevu", 4, "21.10.2026 10:20 için yeni randevu talebi.", false, new DateTime(2026, 10, 15, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "Sistem", 1, "Örnek veri yüklendi.", true, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Doktorlar",
                columns: new[] { "Id", "BolumId", "KullaniciId", "OlusturmaTarihi", "Unvan" },
                values: new object[,]
                {
                    { 1, 1, 2, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Uzm. Dr." },
                    { 2, 1, 3, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Doç. Dr." },
                    { 3, 2, 4, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Prof. Dr." },
                    { 4, 2, 5, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Uzm. Dr." },
                    { 5, 3, 6, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Uzm. Dr." },
                    { 6, 4, 7, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Op. Dr." },
                    { 7, 5, 8, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Op. Dr." },
                    { 8, 6, 9, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Doç. Dr." },
                    { 9, 7, 10, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Uzm. Dr." },
                    { 10, 8, 11, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Uzm. Dr." }
                });

            migrationBuilder.InsertData(
                table: "Hastalar",
                columns: new[] { "Id", "Cinsiyet", "DogumTarihi", "KullaniciId", "OlusturmaTarihi", "TcKimlikNo" },
                values: new object[,]
                {
                    { 1, 1, new DateOnly(1990, 3, 12), 12, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "99000000001" },
                    { 2, 2, new DateOnly(1985, 7, 4), 13, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "99000000002" },
                    { 3, 1, new DateOnly(2001, 11, 23), 14, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "99000000003" },
                    { 4, 2, new DateOnly(1978, 1, 30), 15, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "99000000004" },
                    { 5, 1, new DateOnly(1995, 5, 17), 16, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "99000000005" },
                    { 6, 2, new DateOnly(1969, 9, 8), 17, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "99000000006" },
                    { 7, 1, new DateOnly(2003, 2, 14), 18, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "99000000007" },
                    { 8, 0, new DateOnly(1958, 12, 1), 19, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc), "99000000008" }
                });

            migrationBuilder.InsertData(
                table: "BelirtiKayitlari",
                columns: new[] { "Id", "Aciklama", "Aciliyet", "HastaId", "OlusturmaTarihi", "OnerilenBolumId" },
                values: new object[,]
                {
                    { 1, "Üç gündür süren baş ağrısı ve baş dönmesi.", 2, 1, new DateTime(2026, 10, 3, 8, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 2, "Merdiven çıkarken göğüs ağrısı ve çarpıntı.", 3, 2, new DateTime(2026, 10, 4, 8, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 3, "Boğaz ve kulak ağrısı, hafif ateş.", 1, 3, new DateTime(2026, 10, 6, 8, 0, 0, 0, DateTimeKind.Utc), 4 },
                    { 4, "Bel ağrısı bacağa vuruyor.", 2, 4, new DateTime(2026, 10, 9, 8, 0, 0, 0, DateTimeKind.Utc), 6 },
                    { 5, "Kollarda kaşıntılı döküntü.", 1, 5, new DateTime(2026, 10, 11, 8, 0, 0, 0, DateTimeKind.Utc), 7 },
                    { 6, "Karın ağrısı ve bulantı.", 2, 1, new DateTime(2026, 10, 14, 8, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "Randevular",
                columns: new[] { "Id", "BaslangicZamani", "BelirtiKaydiId", "BitisZamani", "DoktorId", "DoktorNotu", "Durum", "HastaId", "OlusturmaTarihi" },
                values: new object[,]
                {
                    { 5, new DateTime(2026, 10, 13, 9, 40, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 13, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, null, 4, 6, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, new DateTime(2026, 10, 21, 10, 20, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 21, 10, 40, 0, 0, DateTimeKind.Unspecified), 3, null, 2, 2, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, new DateTime(2026, 10, 22, 15, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 22, 15, 20, 0, 0, DateTimeKind.Unspecified), 7, null, 1, 7, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, new DateTime(2026, 10, 23, 9, 20, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 23, 9, 40, 0, 0, DateTimeKind.Unspecified), 10, null, 1, 8, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "BelirtiKaydiBelirtileri",
                columns: new[] { "BelirtiId", "BelirtiKaydiId", "Siddet" },
                values: new object[,]
                {
                    { 1, 1, 7 },
                    { 2, 1, 5 },
                    { 3, 2, 8 },
                    { 4, 2, 6 },
                    { 7, 3, 3 },
                    { 10, 3, 6 },
                    { 11, 3, 5 },
                    { 14, 4, 8 },
                    { 15, 5, 4 },
                    { 8, 6, 6 },
                    { 9, 6, 5 }
                });

            migrationBuilder.InsertData(
                table: "Randevular",
                columns: new[] { "Id", "BaslangicZamani", "BelirtiKaydiId", "BitisZamani", "DoktorId", "DoktorNotu", "Durum", "HastaId", "OlusturmaTarihi" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 5, 9, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 10, 5, 9, 20, 0, 0, DateTimeKind.Unspecified), 5, "Gerilim tipi baş ağrısı. Kontrol önerildi.", 3, 1, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 10, 6, 10, 20, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 10, 6, 10, 40, 0, 0, DateTimeKind.Unspecified), 3, "EKG ve efor testi istendi.", 3, 2, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2026, 10, 8, 14, 0, 0, 0, DateTimeKind.Unspecified), 3, new DateTime(2026, 10, 8, 14, 20, 0, 0, DateTimeKind.Unspecified), 6, "Üst solunum yolu enfeksiyonu.", 3, 3, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2026, 10, 12, 11, 0, 0, 0, DateTimeKind.Unspecified), 4, new DateTime(2026, 10, 12, 11, 20, 0, 0, DateTimeKind.Unspecified), 8, null, 5, 4, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2026, 10, 20, 13, 0, 0, 0, DateTimeKind.Unspecified), 5, new DateTime(2026, 10, 20, 13, 20, 0, 0, DateTimeKind.Unspecified), 9, null, 2, 5, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2026, 10, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), 6, new DateTime(2026, 10, 20, 9, 20, 0, 0, DateTimeKind.Unspecified), 1, null, 1, 1, new DateTime(2026, 10, 1, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BelirtiBolumleri_BolumId",
                table: "BelirtiBolumleri",
                column: "BolumId");

            migrationBuilder.CreateIndex(
                name: "IX_BelirtiKaydiBelirtileri_BelirtiId",
                table: "BelirtiKaydiBelirtileri",
                column: "BelirtiId");

            migrationBuilder.CreateIndex(
                name: "IX_BelirtiKayitlari_HastaId_OlusturmaTarihi",
                table: "BelirtiKayitlari",
                columns: new[] { "HastaId", "OlusturmaTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_BelirtiKayitlari_OnerilenBolumId",
                table: "BelirtiKayitlari",
                column: "OnerilenBolumId");

            migrationBuilder.CreateIndex(
                name: "IX_Belirtiler_Ad",
                table: "Belirtiler",
                column: "Ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_KullaniciId_OkunduMu",
                table: "Bildirimler",
                columns: new[] { "KullaniciId", "OkunduMu" });

            migrationBuilder.CreateIndex(
                name: "IX_Bolumler_Ad",
                table: "Bolumler",
                column: "Ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doktorlar_BolumId",
                table: "Doktorlar",
                column: "BolumId");

            migrationBuilder.CreateIndex(
                name: "IX_Doktorlar_KullaniciId",
                table: "Doktorlar",
                column: "KullaniciId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hastalar_KullaniciId",
                table: "Hastalar",
                column: "KullaniciId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hastalar_TcKimlikNo",
                table: "Hastalar",
                column: "TcKimlikNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_Eposta",
                table: "Kullanicilar",
                column: "Eposta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_BelirtiKaydiId",
                table: "Randevular",
                column: "BelirtiKaydiId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_DoktorId_BaslangicZamani",
                table: "Randevular",
                columns: new[] { "DoktorId", "BaslangicZamani" });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HastaId_BaslangicZamani",
                table: "Randevular",
                columns: new[] { "HastaId", "BaslangicZamani" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BelirtiBolumleri");

            migrationBuilder.DropTable(
                name: "BelirtiKaydiBelirtileri");

            migrationBuilder.DropTable(
                name: "Bildirimler");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "Belirtiler");

            migrationBuilder.DropTable(
                name: "BelirtiKayitlari");

            migrationBuilder.DropTable(
                name: "Doktorlar");

            migrationBuilder.DropTable(
                name: "Hastalar");

            migrationBuilder.DropTable(
                name: "Bolumler");

            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}
