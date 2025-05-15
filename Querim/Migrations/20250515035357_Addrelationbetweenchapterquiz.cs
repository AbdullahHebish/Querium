using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Querim.Migrations
{
    /// <inheritdoc />
    public partial class Addrelationbetweenchapterquiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfPath",
                table: "Chapters");

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        

            migrationBuilder.AddColumn<string>(
                name: "PdfPath",
                table: "Chapters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
