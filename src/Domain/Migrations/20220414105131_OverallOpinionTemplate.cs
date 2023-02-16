using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    public partial class OverallOpinionTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OverallOpinion",
                table: "InterviewTemplates",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OverallOpinion",
                table: "Interviews",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverallOpinion",
                table: "InterviewTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "OverallOpinion",
                table: "Interviews",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20000)",
                oldMaxLength: 20000,
                oldNullable: true);
        }
    }
}
