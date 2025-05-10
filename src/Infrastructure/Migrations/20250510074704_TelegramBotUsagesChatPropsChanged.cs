using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class TelegramBotUsagesChatPropsChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelegramBotUsages_Username_UsageType",
                table: "TelegramBotUsages");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "TelegramBotUsages");

            migrationBuilder.DropColumn(
                name: "ChannelName",
                table: "TelegramBotUsages");

            migrationBuilder.AlterColumn<long>(
                name: "ChatId",
                table: "TelegramBotUsages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramBotUsages_ChatId",
                table: "TelegramBotUsages",
                column: "ChatId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelegramBotUsages_ChatId",
                table: "TelegramBotUsages");

            migrationBuilder.AlterColumn<long>(
                name: "ChatId",
                table: "TelegramBotUsages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ChannelId",
                table: "TelegramBotUsages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChannelName",
                table: "TelegramBotUsages",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramBotUsages_Username_UsageType",
                table: "TelegramBotUsages",
                columns: new[] { "Username", "UsageType" });
        }
    }
}