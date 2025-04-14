using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Querim.Migrations
{
    /// <inheritdoc />
    public partial class Uniquecolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_NationalIDCard",
                table: "Students",
                column: "NationalIDCard",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UniversityIDCard",
                table: "Students",
                column: "UniversityIDCard",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_NationalIDCard",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_UniversityIDCard",
                table: "Students");
        }
    }
}
