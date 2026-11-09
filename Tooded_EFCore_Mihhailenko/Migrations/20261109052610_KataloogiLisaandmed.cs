using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_EFCore.Migrations
{
    /// <inheritdoc />
    public partial class KataloogiLisaandmed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LisainfoJson",
                table: "Toodetabel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LisainfoJson",
                table: "Kategooriatabel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SisuJson",
                table: "KataloogiSeaded",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Kategooriatabel",
                keyColumn: "Id",
                keyValue: 1,
                column: "LisainfoJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Kategooriatabel",
                keyColumn: "Id",
                keyValue: 2,
                column: "LisainfoJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Kategooriatabel",
                keyColumn: "Id",
                keyValue: 3,
                column: "LisainfoJson",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LisainfoJson",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "LisainfoJson",
                table: "Kategooriatabel");

            migrationBuilder.DropColumn(
                name: "SisuJson",
                table: "KataloogiSeaded");
        }
    }
}
