using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class TelegramChannelMonitoringEntitiesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoredChannels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelExternalId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DiscussionChatExternalId = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramRawUpdates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UpdateId = table.Column<long>(type: "bigint", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramRawUpdates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelPosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonitoredChannelId = table.Column<long>(type: "bigint", nullable: false),
                    TelegramMessageId = table.Column<long>(type: "bigint", nullable: false),
                    PostedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TextPreview = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelPosts_MonitoredChannels_MonitoredChannelId",
                        column: x => x.MonitoredChannelId,
                        principalTable: "MonitoredChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyStatsRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonitoredChannelId = table.Column<long>(type: "bigint", nullable: false),
                    Month = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TriggerSource = table.Column<int>(type: "integer", nullable: false),
                    CalculatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostsCountTotal = table.Column<int>(type: "integer", nullable: false),
                    AveragePostsPerDay = table.Column<double>(type: "double precision", nullable: false),
                    MostLikedPostId = table.Column<long>(type: "bigint", nullable: true),
                    MostLikedPostRef = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MostLikedPostLikes = table.Column<int>(type: "integer", nullable: true),
                    MostCommentedPostId = table.Column<long>(type: "bigint", nullable: true),
                    MostCommentedPostRef = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MostCommentedPostComments = table.Column<int>(type: "integer", nullable: true),
                    MaxPostsDayUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MaxPostsCount = table.Column<int>(type: "integer", nullable: false),
                    MinPostsDayUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MinPostsCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyStatsRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyStatsRuns_MonitoredChannels_MonitoredChannelId",
                        column: x => x.MonitoredChannelId,
                        principalTable: "MonitoredChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPosts_MonitoredChannelId_TelegramMessageId",
                table: "ChannelPosts",
                columns: new[] { "MonitoredChannelId", "TelegramMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPosts_PostedAtUtc",
                table: "ChannelPosts",
                column: "PostedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredChannels_ChannelExternalId",
                table: "MonitoredChannels",
                column: "ChannelExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredChannels_DiscussionChatExternalId",
                table: "MonitoredChannels",
                column: "DiscussionChatExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredChannels_IsActive",
                table: "MonitoredChannels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyStatsRuns_MonitoredChannelId_Month",
                table: "MonthlyStatsRuns",
                columns: new[] { "MonitoredChannelId", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramRawUpdates_Status",
                table: "TelegramRawUpdates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramRawUpdates_UpdateId",
                table: "TelegramRawUpdates",
                column: "UpdateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelPosts");

            migrationBuilder.DropTable(
                name: "MonthlyStatsRuns");

            migrationBuilder.DropTable(
                name: "TelegramRawUpdates");

            migrationBuilder.DropTable(
                name: "MonitoredChannels");
        }
    }
}
