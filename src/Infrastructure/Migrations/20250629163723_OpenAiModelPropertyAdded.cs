using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class OpenAiModelPropertyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "OpenAiPrompts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "CompanyOpenAiAnalysisRecords",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "OpenAiPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Model",
                value: "gpt-4o");

            migrationBuilder.UpdateData(
                table: "OpenAiPrompts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Model",
                value: "gpt-4o");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "OpenAiPrompts");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "CompanyOpenAiAnalysisRecords");
        }
    }
}