using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class WorkIndustryEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "WorkIndustryId",
                table: "UserSalaries",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkIndustries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HexColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkIndustries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkIndustries_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSalaries_WorkIndustryId",
                table: "UserSalaries",
                column: "WorkIndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkIndustries_CreatedById",
                table: "WorkIndustries",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSalaries_WorkIndustries_WorkIndustryId",
                table: "UserSalaries",
                column: "WorkIndustryId",
                principalTable: "WorkIndustries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSalaries_WorkIndustries_WorkIndustryId",
                table: "UserSalaries");

            migrationBuilder.DropTable(
                name: "WorkIndustries");

            migrationBuilder.DropIndex(
                name: "IX_UserSalaries_WorkIndustryId",
                table: "UserSalaries");

            migrationBuilder.DropColumn(
                name: "WorkIndustryId",
                table: "UserSalaries");
        }
    }
}