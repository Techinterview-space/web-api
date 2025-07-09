using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class CompanyReviewTgSubscriptionRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // mgorbatyuk: manually edited migration to rename table
            migrationBuilder.DropForeignKey(
                name: "FK_AiAnalysisRecords_LastWeekCompanyReviewsSubscription_Compan~",
                table: "AiAnalysisRecords");

            migrationBuilder.Sql(
                @"DROP TABLE if exists LastWeekCompanyReviewsSubscription cascade;");

            migrationBuilder.CreateTable(
                name: "LastWeekCompanyReviewsSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: false),
                    UseAiAnalysis = table.Column<bool>(type: "boolean", nullable: false),
                    Regularity = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastWeekCompanyReviewsSubscriptions", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AiAnalysisRecords_LastWeekCompanyReviewsSubscription_Compan~",
                table: "AiAnalysisRecords",
                column: "CompanyReviewsSubscriptionId",
                principalTable: "LastWeekCompanyReviewsSubscriptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiAnalysisRecords_LastWeekCompanyReviewsSubscriptions_Compa~",
                table: "AiAnalysisRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LastWeekCompanyReviewsSubscriptions",
                table: "LastWeekCompanyReviewsSubscriptions");

            migrationBuilder.RenameTable(
                name: "LastWeekCompanyReviewsSubscriptions",
                newName: "LastWeekCompanyReviewsSubscription");

            migrationBuilder.AlterColumn<bool>(
                name: "UseAiAnalysis",
                table: "LastWeekCompanyReviewsSubscription",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Regularity",
                table: "LastWeekCompanyReviewsSubscription",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Weekly");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LastWeekCompanyReviewsSubscription",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LastWeekCompanyReviewsSubscription",
                table: "LastWeekCompanyReviewsSubscription",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AiAnalysisRecords_LastWeekCompanyReviewsSubscription_Compan~",
                table: "AiAnalysisRecords",
                column: "CompanyReviewsSubscriptionId",
                principalTable: "LastWeekCompanyReviewsSubscription",
                principalColumn: "Id");
        }
    }
}