using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class ProfessionAsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Profession",
                table: "UserSalaries",
                newName: "ProfessionEnum");

            migrationBuilder.AddColumn<long>(
                name: "ProfessionId",
                table: "UserSalaries",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Professions",
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
                    table.PrimaryKey("PK_Professions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Professions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Professions",
                columns: new[] { "Id", "CreatedById", "HexColor", "Title" },
                values: new object[,]
                {
                    { 1L, null, "#E880C7", "Developer" },
                    { 2L, null, "#D6B770", "QualityAssurance" },
                    { 3L, null, "#708CFA", "Tester" },
                    { 4L, null, "#B7D757", "BusinessAnalyst" },
                    { 5L, null, "#3CFD39", "ProjectManager" },
                    { 6L, null, "#5F4D96", "ScrumMaster" },
                    { 7L, null, "#1BAB17", "DevOps" },
                    { 8L, null, "#5E69D5", "SystemAdministrator" },
                    { 9L, null, "#6435AB", "ProductOwner" },
                    { 10L, null, "#7577F0", "TeamLeader" },
                    { 11L, null, "#965ED9", "Architect" },
                    { 12L, null, "#BB2887", "DataScientist" },
                    { 13L, null, "#AEB283", "DataAnalyst" },
                    { 14L, null, "#EBB854", "DataEngineer" },
                    { 15L, null, "#66F664", "DataWarehouseSpecialist" },
                    { 16L, null, "#B0642B", "DatabaseAdministrator" },
                    { 17L, null, "#F4EBA7", "TechLeader" },
                    { 18L, null, "#B60D33", "SystemAnalyst" },
                    { 19L, null, "#05670E", "ItHr" },
                    { 20L, null, "#BABE97", "ItRecruiter" },
                    { 21L, null, "#71E4FC", "UiDesigner" },
                    { 22L, null, "#CB08CC", "UxDesigner" },
                    { 23L, null, "#EFCE9D", "UiUxDesigner" },
                    { 24L, null, "#B9CA5D", "ProductAnalyst" },
                    { 25L, null, "#752D61", "ProductManager" },
                    { 26L, null, "#B68CD2", "ProductDesigner" },
                    { 27L, null, "#2E9531", "HrNonIt" },
                    { 28L, null, "#43225B", "OneCDeveloper" },
                    { 29L, null, "#6A9826", "ThreeDModeler" },
                    { 30L, null, "#79C38A", "AndroidDeveloper" },
                    { 31L, null, "#6AC243", "IosDeveloper" },
                    { 32L, null, "#66D96D", "MobileDeveloper" },
                    { 33L, null, "#4AEA53", "FrontendDeveloper" },
                    { 34L, null, "#B52DBD", "BackendDeveloper" },
                    { 35L, null, "#AA2681", "FullstackDeveloper" },
                    { 36L, null, "#EED7DF", "GameDeveloper" },
                    { 37L, null, "#074997", "EmbeddedDeveloper" },
                    { 38L, null, "#2CC20E", "MachineLearningDeveloper" },
                    { 39L, null, "#B7C0CC", "Pentester" },
                    { 40L, null, "#9B59F7", "SecurityEngineer" },
                    { 41L, null, "#CA76EE", "SecurityAnalyst" },
                    { 42L, null, "#B8967B", "TechnicalWriter" },
                    { 43L, null, "#F01A7E", "BiDeveloper" },
                    { 44L, null, "#E7853F", "ChiefTechnicalOfficer" },
                    { 45L, null, "#0AB8C9", "ChiefExecutiveOfficer" },
                    { 46L, null, "#93F1F4", "HeadOfDepartment" },
                    { 47L, null, "#B5D4BB", "DeliveryManager" },
                    { 48L, null, "#8090B6", "Copywriter" },
                    { 49L, null, "#009130", "GameDesigner" },
                    { 50L, null, "#E883EF", "SecOps" },
                    { 51L, null, "#35C9A5", "TalentAcquisition" },
                    { 52L, null, "#1FF4AD", "CustomerSupport" },
                    { 53L, null, "#8EF1D5", "TechnicalSupport" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSalaries_ProfessionId",
                table: "UserSalaries",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Professions_CreatedById",
                table: "Professions",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSalaries_Professions_ProfessionId",
                table: "UserSalaries",
                column: "ProfessionId",
                principalTable: "Professions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSalaries_Professions_ProfessionId",
                table: "UserSalaries");

            migrationBuilder.DropTable(
                name: "Professions");

            migrationBuilder.DropIndex(
                name: "IX_UserSalaries_ProfessionId",
                table: "UserSalaries");

            migrationBuilder.DropColumn(
                name: "ProfessionId",
                table: "UserSalaries");

            migrationBuilder.RenameColumn(
                name: "ProfessionEnum",
                table: "UserSalaries",
                newName: "Profession");
        }
    }
}
