using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class StatDataEntitiesAddedAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatDataCacheItems");

            migrationBuilder.DropTable(
                name: "StatDataCache");

            migrationBuilder.CreateTable(
                name: "StatDataChangeSubscriptions",
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
                    table.PrimaryKey("PK_StatDataChangeSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatDataChangeSubscriptionRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatDataChangeSubscriptionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatDataChangeSubscriptionRecord_PreviousRecord",
                        column: x => x.PreviousRecordId,
                        principalTable: "StatDataChangeSubscriptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StatDataChangeSubscriptionRecord_Subscription",
                        column: x => x.SubscriptionId,
                        principalTable: "StatDataChangeSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatDataChangeSubscriptionRecords_PreviousRecordId",
                table: "StatDataChangeSubscriptionRecords",
                column: "PreviousRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_StatDataChangeSubscriptionRecords_SubscriptionId",
                table: "StatDataChangeSubscriptionRecords",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatDataChangeSubscriptionRecords");

            migrationBuilder.DropTable(
                name: "StatDataChangeSubscriptions");

            migrationBuilder.CreateTable(
                name: "StatDataCache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProfessionIds = table.Column<string>(type: "text", nullable: true),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: false),
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
                    PreviousStatDataCacheItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatDataCacheId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true),
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
                column: "PreviousStatDataCacheItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StatDataCacheItems_StatDataCacheId",
                table: "StatDataCacheItems",
                column: "StatDataCacheId");
        }
    }
}