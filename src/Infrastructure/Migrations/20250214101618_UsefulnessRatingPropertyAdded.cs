using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class UsefulnessRatingPropertyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectationReply",
                table: "SalariesSurveyReplies");

            migrationBuilder.RenameColumn(
                name: "UsefulnessReply",
                table: "SalariesSurveyReplies",
                newName: "UsefulnessRating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UsefulnessRating",
                table: "SalariesSurveyReplies",
                newName: "UsefulnessReply");

            migrationBuilder.AddColumn<int>(
                name: "ExpectationReply",
                table: "SalariesSurveyReplies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}