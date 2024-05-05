using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdjustedSurveyProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalariesSurveyReplies_SalariesSurveyQuestions_SalariesSurve~",
                table: "SalariesSurveyReplies");

            migrationBuilder.DropTable(
                name: "SalariesSurveyQuestions");

            migrationBuilder.DropIndex(
                name: "IX_SalariesSurveyReplies_SalariesSurveyQuestionId",
                table: "SalariesSurveyReplies");

            migrationBuilder.DropColumn(
                name: "SalariesSurveyQuestionId",
                table: "SalariesSurveyReplies");

            migrationBuilder.RenameColumn(
                name: "ReplyType",
                table: "SalariesSurveyReplies",
                newName: "UsefulnessReply");

            migrationBuilder.AddColumn<int>(
                name: "ExpectationReply",
                table: "SalariesSurveyReplies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectationReply",
                table: "SalariesSurveyReplies");

            migrationBuilder.RenameColumn(
                name: "UsefulnessReply",
                table: "SalariesSurveyReplies",
                newName: "ReplyType");

            migrationBuilder.AddColumn<Guid>(
                name: "SalariesSurveyQuestionId",
                table: "SalariesSurveyReplies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SalariesSurveyQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalariesSurveyQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalariesSurveyQuestions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "SalariesSurveyQuestions",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Description", "Title", "UpdatedAt" },
                values: new object[] { new Guid("703dc7ad-1395-4c09-bb76-a08b639134e4"), new DateTimeOffset(new DateTime(2024, 4, 30, 5, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 5, 0, 0, 0)), null, null, "Ожидали ли вы, что медианная и средняя зарплаты будут такими, какими они оказались?", new DateTimeOffset(new DateTime(2024, 4, 30, 5, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 5, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_SalariesSurveyReplies_SalariesSurveyQuestionId",
                table: "SalariesSurveyReplies",
                column: "SalariesSurveyQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalariesSurveyQuestions_CreatedByUserId",
                table: "SalariesSurveyQuestions",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalariesSurveyReplies_SalariesSurveyQuestions_SalariesSurve~",
                table: "SalariesSurveyReplies",
                column: "SalariesSurveyQuestionId",
                principalTable: "SalariesSurveyQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}