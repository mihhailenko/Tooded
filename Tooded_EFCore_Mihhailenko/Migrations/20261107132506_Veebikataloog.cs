using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_EFCore.Migrations
{
    /// <inheritdoc />
    public partial class Veebikataloog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "KohalikMuudatus",
                table: "Toodetabel",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Margis",
                table: "Toodetabel",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PildiUrl",
                table: "Toodetabel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tavahind",
                table: "Toodetabel",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uhik",
                table: "Toodetabel",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "tk");

            migrationBuilder.AddColumn<string>(
                name: "ValineId",
                table: "Toodetabel",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Jarjestus",
                table: "Kategooriatabel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ValineId",
                table: "Kategooriatabel",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YlemId",
                table: "Kategooriatabel",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KataloogiSeaded",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    KataloogiId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Versioon = table.Column<long>(type: "bigint", nullable: false),
                    ETag = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SisuHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ViimaneEdu = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ViimaneKontroll = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReaVersioon = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KataloogiSeaded", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Kategooriatabel",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Jarjestus", "ValineId", "YlemId" },
                values: new object[] { 0, null, null });

            migrationBuilder.UpdateData(
                table: "Kategooriatabel",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Jarjestus", "ValineId", "YlemId" },
                values: new object[] { 0, null, null });

            migrationBuilder.UpdateData(
                table: "Kategooriatabel",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Jarjestus", "ValineId", "YlemId" },
                values: new object[] { 0, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Toodetabel_ValineId",
                table: "Toodetabel",
                column: "ValineId",
                unique: true,
                filter: "[ValineId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Kategooriatabel_ValineId",
                table: "Kategooriatabel",
                column: "ValineId",
                unique: true,
                filter: "[ValineId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Kategooriatabel_YlemId",
                table: "Kategooriatabel",
                column: "YlemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Kategooriatabel_Kategooriatabel_YlemId",
                table: "Kategooriatabel",
                column: "YlemId",
                principalTable: "Kategooriatabel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kategooriatabel_Kategooriatabel_YlemId",
                table: "Kategooriatabel");

            migrationBuilder.DropTable(
                name: "KataloogiSeaded");

            migrationBuilder.DropIndex(
                name: "IX_Toodetabel_ValineId",
                table: "Toodetabel");

            migrationBuilder.DropIndex(
                name: "IX_Kategooriatabel_ValineId",
                table: "Kategooriatabel");

            migrationBuilder.DropIndex(
                name: "IX_Kategooriatabel_YlemId",
                table: "Kategooriatabel");

            migrationBuilder.DropColumn(
                name: "KohalikMuudatus",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Margis",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "PildiUrl",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Tavahind",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Uhik",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "ValineId",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Jarjestus",
                table: "Kategooriatabel");

            migrationBuilder.DropColumn(
                name: "ValineId",
                table: "Kategooriatabel");

            migrationBuilder.DropColumn(
                name: "YlemId",
                table: "Kategooriatabel");
        }
    }
}
