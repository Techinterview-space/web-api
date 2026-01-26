using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AiAnalysisEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseAiAnalysis",
                table: "StatDataChangeSubscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AiAnalysisRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AiReportSource = table.Column<string>(type: "text", nullable: false),
                    AiReport = table.Column<string>(type: "text", nullable: false),
                    ProcessingTimeMs = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalysisRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiAnalysisRecords_StatDataChangeSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "StatDataChangeSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalysisRecords_SubscriptionId",
                table: "AiAnalysisRecords",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAnalysisRecords");

            migrationBuilder.DropColumn(
                name: "UseAiAnalysis",
                table: "StatDataChangeSubscriptions");
        }
    }
}