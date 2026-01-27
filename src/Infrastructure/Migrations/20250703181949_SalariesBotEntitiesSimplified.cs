using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class SalariesBotEntitiesSimplified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramBotUsages");

            migrationBuilder.DropColumn(
                name: "ChatName",
                table: "TelegramInlineReplies");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TelegramInlineReplies");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "TelegramInlineReplies");

            migrationBuilder.AddColumn<int>(
                name: "BotType",
                table: "TelegramInlineReplies",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalariesBotMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    UsageType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalariesBotMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalariesBotMessages_ChatId",
                table: "SalariesBotMessages",
                column: "ChatId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalariesBotMessages");

            migrationBuilder.DropColumn(
                name: "BotType",
                table: "TelegramInlineReplies");

            migrationBuilder.AddColumn<string>(
                name: "ChatName",
                table: "TelegramInlineReplies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "TelegramInlineReplies",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "TelegramInlineReplies",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TelegramBotUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedMessageText = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UsageCount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    UsageType = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramBotUsages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramBotUsages_ChatId",
                table: "TelegramBotUsages",
                column: "ChatId",
                unique: true);
        }
    }
}
