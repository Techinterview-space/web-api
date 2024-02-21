using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemovedOrganizationRelatedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Organizations_OrganizationId",
                table: "Interviews");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewTemplates_Organizations_OrganizationId",
                table: "InterviewTemplates");

            migrationBuilder.DropTable(
                name: "CandidateCardComments");

            migrationBuilder.DropTable(
                name: "CandidateCardOrganizationLabel");

            migrationBuilder.DropTable(
                name: "CandidateInterviews");

            migrationBuilder.DropTable(
                name: "JoinToOrgInvitations");

            migrationBuilder.DropTable(
                name: "OrganizationUsers");

            migrationBuilder.DropTable(
                name: "OrganizationLabels");

            migrationBuilder.DropTable(
                name: "CandidateCards");

            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropTable(
                name: "Organizations");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Contacts = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CvFiles = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidates_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Candidates_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JoinToOrgInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedUserId = table.Column<long>(type: "bigint", nullable: false),
                    InviterId = table.Column<long>(type: "bigint", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinToOrgInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JoinToOrgInvitations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JoinToOrgInvitations_Users_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JoinToOrgInvitations_Users_InviterId",
                        column: x => x.InviterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationLabels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    HexColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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
                name: "OrganizationUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUsers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenById = table.Column<long>(type: "bigint", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EmploymentStatus = table.Column<int>(type: "integer", nullable: false),
                    Files = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateCards_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateCards_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateCards_Users_OpenById",
                        column: x => x.OpenById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CandidateCardComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuthorId = table.Column<long>(type: "bigint", nullable: false),
                    CandidateCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "CandidateInterviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizedById = table.Column<long>(type: "bigint", nullable: true),
                    Comments = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    ConductedDuringStatus = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateInterviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateInterviews_CandidateCards_CandidateCardId",
                        column: x => x.CandidateCardId,
                        principalTable: "CandidateCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateInterviews_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CandidateInterviews_Users_OrganizedById",
                        column: x => x.OrganizedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewTemplates_OrganizationId",
                table: "InterviewTemplates",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_OrganizationId",
                table: "Interviews",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCardComments_AuthorId",
                table: "CandidateCardComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCardComments_CandidateCardId",
                table: "CandidateCardComments",
                column: "CandidateCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCardOrganizationLabel_LabelsId",
                table: "CandidateCardOrganizationLabel",
                column: "LabelsId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCards_CandidateId",
                table: "CandidateCards",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCards_OpenById",
                table: "CandidateCards",
                column: "OpenById");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCards_OrganizationId",
                table: "CandidateCards",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviews_CandidateCardId",
                table: "CandidateInterviews",
                column: "CandidateCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviews_InterviewId",
                table: "CandidateInterviews",
                column: "InterviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviews_OrganizedById",
                table: "CandidateInterviews",
                column: "OrganizedById");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_CreatedById",
                table: "Candidates",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_OrganizationId",
                table: "Candidates",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_JoinToOrgInvitations_InvitedUserId",
                table: "JoinToOrgInvitations",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JoinToOrgInvitations_InviterId",
                table: "JoinToOrgInvitations",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_JoinToOrgInvitations_OrganizationId",
                table: "JoinToOrgInvitations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLabels_CreatedById",
                table: "OrganizationLabels",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLabels_OrganizationId",
                table: "OrganizationLabels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ManagerId",
                table: "Organizations",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_OrganizationId",
                table: "OrganizationUsers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_UserId",
                table: "OrganizationUsers",
                column: "UserId");

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
    }
}
