using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    public partial class CandidateCardOrgId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "CandidateCards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CandidateCardComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Comment = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    AuthorId = table.Column<long>(type: "bigint", nullable: false),
                    CandidateCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCardComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateCardComments_CandidateCards_CandidateCardId",
                        column: x => x.CandidateCardId,
                        principalTable: "CandidateCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateCardComments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCards_OrganizationId",
                table: "CandidateCards",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCardComments_AuthorId",
                table: "CandidateCardComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCardComments_CandidateCardId",
                table: "CandidateCardComments",
                column: "CandidateCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateCards_Organizations_OrganizationId",
                table: "CandidateCards",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateCards_Organizations_OrganizationId",
                table: "CandidateCards");

            migrationBuilder.DropTable(
                name: "CandidateCardComments");

            migrationBuilder.DropIndex(
                name: "IX_CandidateCards_OrganizationId",
                table: "CandidateCards");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "CandidateCards");
        }
    }
}
