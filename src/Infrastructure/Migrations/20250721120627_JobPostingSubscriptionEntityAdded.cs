using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class JobPostingSubscriptionEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPostingMessageSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: false),
                    ProfessionIds = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingMessageSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingMessageSubscriptions_TelegramChatId",
                table: "JobPostingMessageSubscriptions",
                column: "TelegramChatId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPostingMessageSubscriptions");
        }
    }
}