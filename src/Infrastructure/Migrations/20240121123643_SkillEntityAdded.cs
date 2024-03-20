using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class SkillEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SkillId",
                table: "UserSalaries",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Skills",
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
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skills_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSalaries_SkillId",
                table: "UserSalaries",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_CreatedById",
                table: "Skills",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSalaries_Skills_SkillId",
                table: "UserSalaries",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSalaries_Skills_SkillId",
                table: "UserSalaries");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_UserSalaries_SkillId",
                table: "UserSalaries");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "UserSalaries");
        }
    }
}