using System;
using AkilliRandevu.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkilliRandevu.DataAccess.Migrations
{
    /// <summary>
    /// Hafta 10: yapay zekâ değerlendirmeleri (YzDegerlendirmeleri). Elle yazıldı; CI'da model uyumu doğrulanır.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261117130000_Hafta10YapayZeka")]
    public partial class Hafta10YapayZeka : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YzDegerlendirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BelirtiKaydiId = table.Column<int>(type: "INTEGER", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    OnerilenBolumId = table.Column<int>(type: "INTEGER", nullable: true),
                    Gerekce = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Guven = table.Column<int>(type: "INTEGER", nullable: true),
                    AcilBelirtiVar = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcilGerekce = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    IstemSurumu = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SureMs = table.Column<int>(type: "INTEGER", nullable: false),
                    Hata = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YzDegerlendirmeleri", x => x.Id);
                    table.CheckConstraint("CK_YzDegerlendirmesi_Durum", "Durum BETWEEN 1 AND 4");
                    table.CheckConstraint("CK_YzDegerlendirmesi_Guven", "Guven IS NULL OR Guven BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_YzDegerlendirmeleri_BelirtiKayitlari_BelirtiKaydiId",
                        column: x => x.BelirtiKaydiId,
                        principalTable: "BelirtiKayitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YzDegerlendirmeleri_Bolumler_OnerilenBolumId",
                        column: x => x.OnerilenBolumId,
                        principalTable: "Bolumler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YzDegerlendirmeleri_BelirtiKaydiId",
                table: "YzDegerlendirmeleri",
                column: "BelirtiKaydiId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YzDegerlendirmeleri_OnerilenBolumId",
                table: "YzDegerlendirmeleri",
                column: "OnerilenBolumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YzDegerlendirmeleri");
        }
    }
}
