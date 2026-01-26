using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class SalariesHistoricalEntitiesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalariesHistoricalDataRecordTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProfessionIds = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalariesHistoricalDataRecordTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalariesHistoricalDataRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalariesHistoricalDataRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalariesHistoricalDataRecords_SalariesHistoricalDataRecordT~",
                        column: x => x.TemplateId,
                        principalTable: "SalariesHistoricalDataRecordTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalariesHistoricalDataRecords_Date",
                table: "SalariesHistoricalDataRecords",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_SalariesHistoricalDataRecords_TemplateId",
                table: "SalariesHistoricalDataRecords",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalariesHistoricalDataRecords");

            migrationBuilder.DropTable(
                name: "SalariesHistoricalDataRecordTemplates");
        }
    }
}