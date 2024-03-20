using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    public partial class OrganizationLabels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationLabels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HexColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationLabels_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationLabels_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CandidateCardOrganizationLabel",
                columns: table => new
                {
                    CardsId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCardOrganizationLabel", x => new { x.CardsId, x.LabelsId });
                    table.ForeignKey(
                        name: "FK_CandidateCardOrganizationLabel_CandidateCards_CardsId",
                        column: x => x.CardsId,
                        principalTable: "CandidateCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateCardOrganizationLabel_OrganizationLabels_LabelsId",
                        column: x => x.LabelsId,
                        principalTable: "OrganizationLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCardOrganizationLabel_LabelsId",
                table: "CandidateCardOrganizationLabel",
                column: "LabelsId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLabels_CreatedById",
                table: "OrganizationLabels",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLabels_OrganizationId",
                table: "OrganizationLabels",
                column: "OrganizationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateCardOrganizationLabel");

            migrationBuilder.DropTable(
                name: "OrganizationLabels");
        }
    }
}
