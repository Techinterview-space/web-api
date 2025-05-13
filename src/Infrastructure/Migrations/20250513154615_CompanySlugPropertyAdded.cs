using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class CompanySlugPropertyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Slug",
                table: "Companies",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_Slug",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Companies");
        }
    }
}