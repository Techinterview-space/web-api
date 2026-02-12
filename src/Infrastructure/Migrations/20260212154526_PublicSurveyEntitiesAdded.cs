using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class PublicSurveyEntitiesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicSurveys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthorId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSurveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicSurveys_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PublicSurveyQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicSurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    AllowMultipleChoices = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSurveyQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicSurveyQuestions_PublicSurveys_PublicSurveyId",
                        column: x => x.PublicSurveyId,
                        principalTable: "PublicSurveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicSurveyOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSurveyOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicSurveyOptions_PublicSurveyQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "PublicSurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicSurveyResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSurveyResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicSurveyResponses_PublicSurveyQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "PublicSurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicSurveyResponseOptions",
                columns: table => new
                {
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSurveyResponseOptions", x => new { x.ResponseId, x.OptionId });
                    table.ForeignKey(
                        name: "FK_PublicSurveyResponseOptions_PublicSurveyOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "PublicSurveyOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublicSurveyResponseOptions_PublicSurveyResponses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "PublicSurveyResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicSurveyOptions_QuestionId_Order",
                table: "PublicSurveyOptions",
                columns: new[] { "QuestionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicSurveyQuestions_PublicSurveyId_Order",
                table: "PublicSurveyQuestions",
                columns: new[] { "PublicSurveyId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicSurveyResponseOptions_OptionId",
                table: "PublicSurveyResponseOptions",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicSurveyResponses_QuestionId_UserId",
                table: "PublicSurveyResponses",
                columns: new[] { "QuestionId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicSurveys_AuthorId",
                table: "PublicSurveys",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicSurveys_Slug",
                table: "PublicSurveys",
                column: "Slug",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PublicSurveys_Status",
                table: "PublicSurveys",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicSurveyResponseOptions");

            migrationBuilder.DropTable(
                name: "PublicSurveyOptions");

            migrationBuilder.DropTable(
                name: "PublicSurveyResponses");

            migrationBuilder.DropTable(
                name: "PublicSurveyQuestions");

            migrationBuilder.DropTable(
                name: "PublicSurveys");
        }
    }
}
