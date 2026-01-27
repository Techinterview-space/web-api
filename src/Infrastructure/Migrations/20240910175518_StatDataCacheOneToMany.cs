using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class StatDataCacheOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StatDataCacheItems_PreviousStatDataCacheItemId",
                table: "StatDataCacheItems");

            migrationBuilder.AddColumn<long>(
                name: "ChannelId",
                table: "TelegramBotUsages",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatDataCacheItems_PreviousStatDataCacheItemId",
                table: "StatDataCacheItems",
                column: "PreviousStatDataCacheItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StatDataCacheItems_PreviousStatDataCacheItemId",
                table: "StatDataCacheItems");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "TelegramBotUsages");

            migrationBuilder.CreateIndex(
                name: "IX_StatDataCacheItems_PreviousStatDataCacheItemId",
                table: "StatDataCacheItems",
                column: "PreviousStatDataCacheItemId",
                unique: true);
        }
    }
}
