using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    public partial class UserLabels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLabels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HexColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLabels_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InterviewTemplateUserLabel",
                columns: table => new
                {
                    InterviewTemplatesId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewTemplateUserLabel", x => new { x.InterviewTemplatesId, x.LabelsId });
                    table.ForeignKey(
                        name: "FK_InterviewTemplateUserLabel_InterviewTemplates_InterviewTemp~",
                        column: x => x.InterviewTemplatesId,
                        principalTable: "InterviewTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewTemplateUserLabel_UserLabels_LabelsId",
                        column: x => x.LabelsId,
                        principalTable: "UserLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewUserLabel",
                columns: table => new
                {
                    InterviewsId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewUserLabel", x => new { x.InterviewsId, x.LabelsId });
                    table.ForeignKey(
                        name: "FK_InterviewUserLabel_Interviews_InterviewsId",
                        column: x => x.InterviewsId,
                        principalTable: "Interviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewUserLabel_UserLabels_LabelsId",
                        column: x => x.LabelsId,
                        principalTable: "UserLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewTemplateUserLabel_LabelsId",
                table: "InterviewTemplateUserLabel",
                column: "LabelsId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewUserLabel_LabelsId",
                table: "InterviewUserLabel",
                column: "LabelsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLabels_CreatedById",
                table: "UserLabels",
                column: "CreatedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewTemplateUserLabel");

            migrationBuilder.DropTable(
                name: "InterviewUserLabel");

            migrationBuilder.DropTable(
                name: "UserLabels");
        }
    }
}
