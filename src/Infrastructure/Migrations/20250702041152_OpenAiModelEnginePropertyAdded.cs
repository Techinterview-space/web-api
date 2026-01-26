using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class OpenAiModelEnginePropertyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "OpenAiPrompts",
                type: "text",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Engine",
                table: "OpenAiPrompts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "OpenAiPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Engine",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OpenAiPrompts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Engine",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Engine",
                table: "OpenAiPrompts");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "OpenAiPrompts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}