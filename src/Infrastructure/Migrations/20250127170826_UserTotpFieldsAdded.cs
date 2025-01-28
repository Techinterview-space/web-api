using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class UserTotpFieldsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TotpSecret",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TotpVerifiedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotpSecret",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotpVerifiedAt",
                table: "Users");
        }
    }
}