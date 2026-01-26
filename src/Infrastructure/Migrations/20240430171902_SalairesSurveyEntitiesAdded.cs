using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class SalairesSurveyEntitiesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalariesSurveyQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "SalariesSurveyReplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplyType = table.Column<int>(type: "integer", nullable: false),
                    SalariesSurveyQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalariesSurveyReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalariesSurveyReplies_SalariesSurveyQuestions_SalariesSurve~",
                        column: x => x.SalariesSurveyQuestionId,
                        principalTable: "SalariesSurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalariesSurveyReplies_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "SalariesSurveyQuestions",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Description", "Title", "UpdatedAt" },
                values: new object[] { new Guid("703dc7ad-1395-4c09-bb76-a08b639134e4"), new DateTimeOffset(new DateTime(2024, 4, 30, 5, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 5, 0, 0, 0)), null, null, "Ожидали ли вы, что медианная и средняя зарплаты будут такими, какими они оказались?", new DateTimeOffset(new DateTime(2024, 4, 30, 5, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 5, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_SalariesSurveyQuestions_CreatedByUserId",
                table: "SalariesSurveyQuestions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalariesSurveyReplies_CreatedByUserId",
                table: "SalariesSurveyReplies",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalariesSurveyReplies_SalariesSurveyQuestionId",
                table: "SalariesSurveyReplies",
                column: "SalariesSurveyQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalariesSurveyReplies");

            migrationBuilder.DropTable(
                name: "SalariesSurveyQuestions");
        }
    }
}