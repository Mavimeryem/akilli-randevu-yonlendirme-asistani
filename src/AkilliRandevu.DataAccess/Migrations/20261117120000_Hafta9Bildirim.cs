using System;
using AkilliRandevu.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkilliRandevu.DataAccess.Migrations
{
    /// <summary>
    /// Hafta 9: e-posta giden kutusu (EpostaGonderimleri) ve randevu hatırlatma işareti.
    /// Not: Bu bilgisayarda Windows Uygulama Denetimi dotnet-ef aracını engellediği için migration elle yazıldı;
    /// model ile uyumu CI'da "dotnet ef migrations has-pending-model-changes" ve testlerle doğrulanır.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261117120000_Hafta9Bildirim")]
    public partial class Hafta9Bildirim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HatirlatmaTarihi",
                table: "Randevular",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EpostaGonderimleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BildirimId = table.Column<int>(type: "INTEGER", nullable: false),
                    Alici = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    DenemeSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SonHata = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DisMesajId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    GonderimTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpostaGonderimleri", x => x.Id);
                    table.CheckConstraint("CK_EpostaGonderimi_Durum", "Durum BETWEEN 1 AND 4");
                    table.ForeignKey(
                        name: "FK_EpostaGonderimleri_Bildirimler_BildirimId",
                        column: x => x.BildirimId,
                        principalTable: "Bildirimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EpostaGonderimleri_BildirimId",
                table: "EpostaGonderimleri",
                column: "BildirimId");

            migrationBuilder.CreateIndex(
                name: "IX_EpostaGonderimleri_Durum_OlusturmaTarihi",
                table: "EpostaGonderimleri",
                columns: new[] { "Durum", "OlusturmaTarihi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EpostaGonderimleri");

            migrationBuilder.DropColumn(
                name: "HatirlatmaTarihi",
                table: "Randevular");
        }
    }
}
