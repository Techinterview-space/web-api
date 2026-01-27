using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class StatDataCacheEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatDataCache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: false),
                    ProfessionIds = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatDataCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatDataCacheItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StatDataCacheId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStatDataCacheItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatDataCacheItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatDataCacheItems_StatDataCacheItems_PreviousStatDataCache~",
                        column: x => x.PreviousStatDataCacheItemId,
                        principalTable: "StatDataCacheItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StatDataCacheItems_StatDataCache_StatDataCacheId",
                        column: x => x.StatDataCacheId,
                        principalTable: "StatDataCache",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatDataCacheItems_PreviousStatDataCacheItemId",
                table: "StatDataCacheItems",
                column: "PreviousStatDataCacheItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatDataCacheItems_StatDataCacheId",
                table: "StatDataCacheItems",
                column: "StatDataCacheId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatDataCacheItems");

            migrationBuilder.DropTable(
                name: "StatDataCache");
        }
    }
}
