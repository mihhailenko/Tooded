using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_EFCore.Migrations
{
    /// <inheritdoc />
    public partial class PostiSeadedAndmebaasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostiSeaded",
                columns: table => new
                {
                    WindowsiKasutaja = table.Column<string>(type: "nvarchar(184)", maxLength: 184, nullable: false),
                    Server = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Saatja = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kasutajanimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KaitstudParool = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostiSeaded", x => x.WindowsiKasutaja);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostiSeaded");
        }
    }
}
