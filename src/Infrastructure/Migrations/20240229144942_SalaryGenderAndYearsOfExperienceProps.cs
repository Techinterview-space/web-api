using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class SalaryGenderAndYearsOfExperienceProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "UserSalaries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearOfStartingWork",
                table: "UserSalaries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "UserSalaries");

            migrationBuilder.DropColumn(
                name: "YearOfStartingWork",
                table: "UserSalaries");
        }
    }
}