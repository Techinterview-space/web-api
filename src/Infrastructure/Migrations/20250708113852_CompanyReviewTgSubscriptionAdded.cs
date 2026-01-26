using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class CompanyReviewTgSubscriptionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiAnalysisRecords_StatDataChangeSubscriptions_SubscriptionId",
                table: "AiAnalysisRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_StatDataChangeSubscriptionTgMessages_StatDataChangeSubscrip~",
                table: "StatDataChangeSubscriptionTgMessages");

            migrationBuilder.DropIndex(
                name: "IX_StatDataChangeSubscriptionTgMessages_SubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyReviewsSubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages",
                newName: "SalarySubscriptionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubscriptionId",
                table: "AiAnalysisRecords",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyReviewsSubscriptionId",
                table: "AiAnalysisRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LastWeekCompanyReviewsSubscription",
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
                    table.PrimaryKey("PK_LastWeekCompanyReviewsSubscription", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatDataChangeSubscriptionTgMessages_CompanyReviewsSubscrip~",
                table: "StatDataChangeSubscriptionTgMessages",
                column: "CompanyReviewsSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_StatDataChangeSubscriptionTgMessages_SalarySubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages",
                column: "SalarySubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalysisRecords_CompanyReviewsSubscriptionId",
                table: "AiAnalysisRecords",
                column: "CompanyReviewsSubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiAnalysisRecords_LastWeekCompanyReviewsSubscription_Compan~",
                table: "AiAnalysisRecords",
                column: "CompanyReviewsSubscriptionId",
                principalTable: "LastWeekCompanyReviewsSubscription",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AiAnalysisRecords_StatDataChangeSubscriptions_SubscriptionId",
                table: "AiAnalysisRecords",
                column: "SubscriptionId",
                principalTable: "StatDataChangeSubscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StatDataChangeSubscriptionTgMessages_LastWeekCompanyReviews~",
                table: "StatDataChangeSubscriptionTgMessages",
                column: "CompanyReviewsSubscriptionId",
                principalTable: "LastWeekCompanyReviewsSubscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StatDataChangeSubscriptionTgMessages_StatDataChangeSubscrip~",
                table: "StatDataChangeSubscriptionTgMessages",
                column: "SalarySubscriptionId",
                principalTable: "StatDataChangeSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiAnalysisRecords_LastWeekCompanyReviewsSubscription_Compan~",
                table: "AiAnalysisRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_AiAnalysisRecords_StatDataChangeSubscriptions_SubscriptionId",
                table: "AiAnalysisRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_StatDataChangeSubscriptionTgMessages_LastWeekCompanyReviews~",
                table: "StatDataChangeSubscriptionTgMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_StatDataChangeSubscriptionTgMessages_StatDataChangeSubscrip~",
                table: "StatDataChangeSubscriptionTgMessages");

            migrationBuilder.DropTable(
                name: "LastWeekCompanyReviewsSubscription");

            migrationBuilder.DropIndex(
                name: "IX_StatDataChangeSubscriptionTgMessages_CompanyReviewsSubscrip~",
                table: "StatDataChangeSubscriptionTgMessages");

            migrationBuilder.DropIndex(
                name: "IX_StatDataChangeSubscriptionTgMessages_SalarySubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages");

            migrationBuilder.DropIndex(
                name: "IX_AiAnalysisRecords_CompanyReviewsSubscriptionId",
                table: "AiAnalysisRecords");

            migrationBuilder.DropColumn(
                name: "CompanyReviewsSubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages");

            migrationBuilder.DropColumn(
                name: "CompanyReviewsSubscriptionId",
                table: "AiAnalysisRecords");

            migrationBuilder.RenameColumn(
                name: "SalarySubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages",
                newName: "SubscriptionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages",
                newName: "SalarySubscriptionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubscriptionId",
                table: "AiAnalysisRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatDataChangeSubscriptionTgMessages_SubscriptionId",
                table: "StatDataChangeSubscriptionTgMessages",
                column: "SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiAnalysisRecords_StatDataChangeSubscriptions_SubscriptionId",
                table: "AiAnalysisRecords",
                column: "SubscriptionId",
                principalTable: "StatDataChangeSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StatDataChangeSubscriptionTgMessages_StatDataChangeSubscrip~",
                table: "StatDataChangeSubscriptionTgMessages",
                column: "SubscriptionId",
                principalTable: "StatDataChangeSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}