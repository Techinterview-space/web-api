using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class GithubProfileRelatedEntitiesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GithubPersonalUserTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubPersonalUserTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GithubProfileBotChats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    MessagesCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubProfileBotChats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GithubProfileProcessingJobs",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubProfileProcessingJobs", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "GithubProfiles",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    RequestsCount = table.Column<int>(type: "integer", nullable: false),
                    DataSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubProfiles", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "GithubProfileBotMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    GithubProfileBotChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubProfileBotMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GithubProfileBotMessages_GithubProfileBotChats_GithubProfil~",
                        column: x => x.GithubProfileBotChatId,
                        principalTable: "GithubProfileBotChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GithubProfileBotChats_ChatId",
                table: "GithubProfileBotChats",
                column: "ChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GithubProfileBotMessages_GithubProfileBotChatId",
                table: "GithubProfileBotMessages",
                column: "GithubProfileBotChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GithubPersonalUserTokens");

            migrationBuilder.DropTable(
                name: "GithubProfileBotMessages");

            migrationBuilder.DropTable(
                name: "GithubProfileProcessingJobs");

            migrationBuilder.DropTable(
                name: "GithubProfiles");

            migrationBuilder.DropTable(
                name: "GithubProfileBotChats");
        }
    }
}