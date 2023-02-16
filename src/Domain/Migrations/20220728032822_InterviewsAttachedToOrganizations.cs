using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    public partial class InterviewsAttachedToOrganizations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "InterviewTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Interviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewTemplates_OrganizationId",
                table: "InterviewTemplates",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_OrganizationId",
                table: "Interviews",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_Organizations_OrganizationId",
                table: "Interviews",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewTemplates_Organizations_OrganizationId",
                table: "InterviewTemplates",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Organizations_OrganizationId",
                table: "Interviews");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewTemplates_Organizations_OrganizationId",
                table: "InterviewTemplates");

            migrationBuilder.DropIndex(
                name: "IX_InterviewTemplates_OrganizationId",
                table: "InterviewTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_OrganizationId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "InterviewTemplates");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Interviews");
        }
    }
}
