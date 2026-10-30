using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_EFCore.Migrations
{
    /// <inheritdoc />
    public partial class TellimusedJaHaldus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Toodetabel_Kategooriatabel_KategooriaId",
                table: "Toodetabel");

            migrationBuilder.AddColumn<bool>(
                name: "Aktiivne",
                table: "Toodetabel",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Kirjeldus",
                table: "Toodetabel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ReaVersioon",
                table: "Toodetabel",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "PeabParooliMuutma",
                table: "Kasutajad",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Olemasolevad tooted jäävad müüki. Vanad kontod peavad parooli üks kord vahetama.
            migrationBuilder.Sql("UPDATE [Toodetabel] SET [Aktiivne] = 1;");
            migrationBuilder.Sql("UPDATE [Kasutajad] SET [PeabParooliMuutma] = 1;");

            migrationBuilder.CreateTable(
                name: "Tellimused",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    OstuTunnus = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kuupaev = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KasutajaId = table.Column<int>(type: "int", nullable: false),
                    OstjaNimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OstjaEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Staatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summa = table.Column<decimal>(type: "decimal(28,2)", precision: 28, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tellimused", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tellimused_Kasutajad_KasutajaId",
                        column: x => x.KasutajaId,
                        principalTable: "Kasutajad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TellimuseRead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TellimusId = table.Column<int>(type: "int", nullable: false),
                    ToodeId = table.Column<int>(type: "int", nullable: false),
                    TooteNimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kogus = table.Column<int>(type: "int", nullable: false),
                    Hind = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Summa = table.Column<decimal>(type: "decimal(28,2)", precision: 28, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TellimuseRead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TellimuseRead_Tellimused_TellimusId",
                        column: x => x.TellimusId,
                        principalTable: "Tellimused",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TellimuseRead_Toodetabel_ToodeId",
                        column: x => x.ToodeId,
                        principalTable: "Toodetabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tellimused_KasutajaId",
                table: "Tellimused",
                column: "KasutajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tellimused_Number",
                table: "Tellimused",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tellimused_OstuTunnus",
                table: "Tellimused",
                column: "OstuTunnus",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TellimuseRead_TellimusId",
                table: "TellimuseRead",
                column: "TellimusId");

            migrationBuilder.CreateIndex(
                name: "IX_TellimuseRead_ToodeId",
                table: "TellimuseRead",
                column: "ToodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Toodetabel_Kategooriatabel_KategooriaId",
                table: "Toodetabel",
                column: "KategooriaId",
                principalTable: "Kategooriatabel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Toodetabel_Kategooriatabel_KategooriaId",
                table: "Toodetabel");

            migrationBuilder.DropTable(
                name: "TellimuseRead");

            migrationBuilder.DropTable(
                name: "Tellimused");

            migrationBuilder.DropColumn(
                name: "Aktiivne",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Kirjeldus",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "ReaVersioon",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "PeabParooliMuutma",
                table: "Kasutajad");

            migrationBuilder.AddForeignKey(
                name: "FK_Toodetabel_Kategooriatabel_KategooriaId",
                table: "Toodetabel",
                column: "KategooriaId",
                principalTable: "Kategooriatabel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
