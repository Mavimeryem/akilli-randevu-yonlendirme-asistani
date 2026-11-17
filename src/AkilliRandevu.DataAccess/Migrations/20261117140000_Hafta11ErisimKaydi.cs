using System;
using AkilliRandevu.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkilliRandevu.DataAccess.Migrations
{
    /// <summary>
    /// Hafta 11: hasta geçmişi erişim izleri (ErisimKayitlari). Elle yazıldı; CI'da model uyumu doğrulanır.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261117140000_Hafta11ErisimKaydi")]
    public partial class Hafta11ErisimKaydi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErisimKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HastaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ErisenKullaniciId = table.Column<int>(type: "INTEGER", nullable: false),
                    ErisenRol = table.Column<int>(type: "INTEGER", nullable: false),
                    Islem = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErisimKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErisimKayitlari_Hastalar_HastaId",
                        column: x => x.HastaId,
                        principalTable: "Hastalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ErisimKayitlari_Kullanicilar_ErisenKullaniciId",
                        column: x => x.ErisenKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErisimKayitlari_ErisenKullaniciId",
                table: "ErisimKayitlari",
                column: "ErisenKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_ErisimKayitlari_HastaId_OlusturmaTarihi",
                table: "ErisimKayitlari",
                columns: new[] { "HastaId", "OlusturmaTarihi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErisimKayitlari");
        }
    }
}
