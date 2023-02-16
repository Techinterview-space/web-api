﻿// <auto-generated />
using System;
using Domain.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220711102341_CandidateCardOrgId")]
    partial class CandidateCardOrgId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Employments.Candidate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Contacts")
                        .HasMaxLength(5000)
                        .HasColumnType("character varying(5000)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("CreatedById")
                        .HasColumnType("bigint");

                    b.Property<string>("CvFiles")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Candidates", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Employments.CandidateCard", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CandidateId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("EmploymentStatus")
                        .HasColumnType("integer");

                    b.Property<long?>("OpenById")
                        .HasColumnType("bigint");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CandidateId");

                    b.HasIndex("OpenById");

                    b.HasIndex("OrganizationId");

                    b.ToTable("CandidateCards", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Employments.CandidateInterview", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CandidateCardId")
                        .HasColumnType("uuid");

                    b.Property<string>("Comments")
                        .HasMaxLength(3000)
                        .HasColumnType("character varying(3000)");

                    b.Property<int?>("ConductedDuringStatus")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("InterviewId")
                        .HasColumnType("uuid");

                    b.Property<long?>("OrganizedById")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CandidateCardId");

                    b.HasIndex("InterviewId")
                        .IsUnique();

                    b.HasIndex("OrganizedById");

                    b.ToTable("CandidateInterviews", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Interviews.Interview", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long?>("CandidateGrade")
                        .HasColumnType("bigint");

                    b.Property<string>("CandidateName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("InterviewerId")
                        .HasColumnType("bigint");

                    b.Property<string>("OverallOpinion")
                        .HasMaxLength(20000)
                        .HasColumnType("character varying(20000)");

                    b.Property<string>("Subjects")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("InterviewerId");

                    b.ToTable("Interviews", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Interviews.InterviewTemplate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("AuthorId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("boolean");

                    b.Property<string>("OverallOpinion")
                        .HasMaxLength(20000)
                        .HasColumnType("character varying(20000)");

                    b.Property<string>("Subjects")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("InterviewTemplates", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Labels.UserLabel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("CreatedById")
                        .HasColumnType("bigint");

                    b.Property<string>("HexColor")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("character varying(7)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.ToTable("UserLabels", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Organizations.JoinToOrgInvitation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("InvitedUserId")
                        .HasColumnType("bigint");

                    b.Property<long>("InviterId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("InvitedUserId");

                    b.HasIndex("InviterId");

                    b.HasIndex("OrganizationId");

                    b.ToTable("JoinToOrgInvitations", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Organizations.Organization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(5000)
                        .HasColumnType("character varying(5000)");

                    b.Property<long?>("ManagerId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ManagerId");

                    b.ToTable("Organizations", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Organizations.OrganizationUser", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.HasIndex("UserId");

                    b.ToTable("OrganizationUsers", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Users.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<long?>("IdentityId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset?>("LastLoginAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("IdentityId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Domain.Entities.Users.UserRole", b =>
                {
                    b.Property<long>("RoleId")
                        .HasColumnType("bigint");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("RoleId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("UserRole");
                });

            modelBuilder.Entity("Domain.Services.Organizations.CandidateCardComment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AuthorId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CandidateCardId")
                        .HasColumnType("uuid");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("character varying(10000)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("CandidateCardId");

                    b.ToTable("CandidateCardComments", (string)null);
                });

            modelBuilder.Entity("InterviewTemplateUserLabel", b =>
                {
                    b.Property<Guid>("InterviewTemplatesId")
                        .HasColumnType("uuid");

                    b.Property<long>("LabelsId")
                        .HasColumnType("bigint");

                    b.HasKey("InterviewTemplatesId", "LabelsId");

                    b.HasIndex("LabelsId");

                    b.ToTable("InterviewTemplateUserLabel");
                });

            modelBuilder.Entity("InterviewUserLabel", b =>
                {
                    b.Property<Guid>("InterviewsId")
                        .HasColumnType("uuid");

                    b.Property<long>("LabelsId")
                        .HasColumnType("bigint");

                    b.HasKey("InterviewsId", "LabelsId");

                    b.HasIndex("LabelsId");

                    b.ToTable("InterviewUserLabel");
                });

            modelBuilder.Entity("Domain.Entities.Employments.Candidate", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.HasOne("Domain.Entities.Organizations.Organization", "Organization")
                        .WithMany("Candidates")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Domain.Entities.Employments.CandidateCard", b =>
                {
                    b.HasOne("Domain.Entities.Employments.Candidate", "Candidate")
                        .WithMany("CandidateCards")
                        .HasForeignKey("CandidateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Users.User", "OpenBy")
                        .WithMany()
                        .HasForeignKey("OpenById");

                    b.HasOne("Domain.Entities.Organizations.Organization", "Organization")
                        .WithMany("CandidateCards")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Candidate");

                    b.Navigation("OpenBy");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Domain.Entities.Employments.CandidateInterview", b =>
                {
                    b.HasOne("Domain.Entities.Employments.CandidateCard", "CandidateCard")
                        .WithMany("Interviews")
                        .HasForeignKey("CandidateCardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Interviews.Interview", "Interview")
                        .WithOne("CandidateInterview")
                        .HasForeignKey("Domain.Entities.Employments.CandidateInterview", "InterviewId");

                    b.HasOne("Domain.Entities.Users.User", "OrganizedBy")
                        .WithMany()
                        .HasForeignKey("OrganizedById");

                    b.Navigation("CandidateCard");

                    b.Navigation("Interview");

                    b.Navigation("OrganizedBy");
                });

            modelBuilder.Entity("Domain.Entities.Interviews.Interview", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "Interviewer")
                        .WithMany()
                        .HasForeignKey("InterviewerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Interviewer");
                });

            modelBuilder.Entity("Domain.Entities.Interviews.InterviewTemplate", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");
                });

            modelBuilder.Entity("Domain.Entities.Labels.UserLabel", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("Domain.Entities.Organizations.JoinToOrgInvitation", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "InvitedUser")
                        .WithMany()
                        .HasForeignKey("InvitedUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Users.User", "Inviter")
                        .WithMany()
                        .HasForeignKey("InviterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Organizations.Organization", "Organization")
                        .WithMany("Invitations")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InvitedUser");

                    b.Navigation("Inviter");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Domain.Entities.Organizations.Organization", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "Manager")
                        .WithMany()
                        .HasForeignKey("ManagerId");

                    b.Navigation("Manager");
                });

            modelBuilder.Entity("Domain.Entities.Organizations.OrganizationUser", b =>
                {
                    b.HasOne("Domain.Entities.Organizations.Organization", "Organization")
                        .WithMany("Users")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Users.User", "User")
                        .WithMany("OrganizationUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Users.UserRole", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Services.Organizations.CandidateCardComment", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Employments.CandidateCard", "CandidateCard")
                        .WithMany("Comments")
                        .HasForeignKey("CandidateCardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("CandidateCard");
                });

            modelBuilder.Entity("InterviewTemplateUserLabel", b =>
                {
                    b.HasOne("Domain.Entities.Interviews.InterviewTemplate", null)
                        .WithMany()
                        .HasForeignKey("InterviewTemplatesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Labels.UserLabel", null)
                        .WithMany()
                        .HasForeignKey("LabelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("InterviewUserLabel", b =>
                {
                    b.HasOne("Domain.Entities.Interviews.Interview", null)
                        .WithMany()
                        .HasForeignKey("InterviewsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Labels.UserLabel", null)
                        .WithMany()
                        .HasForeignKey("LabelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.Employments.Candidate", b =>
                {
                    b.Navigation("CandidateCards");
                });

            modelBuilder.Entity("Domain.Entities.Employments.CandidateCard", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Interviews");
                });

            modelBuilder.Entity("Domain.Entities.Interviews.Interview", b =>
                {
                    b.Navigation("CandidateInterview");
                });

            modelBuilder.Entity("Domain.Entities.Organizations.Organization", b =>
                {
                    b.Navigation("CandidateCards");

                    b.Navigation("Candidates");

                    b.Navigation("Invitations");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Domain.Entities.Users.User", b =>
                {
                    b.Navigation("OrganizationUsers");

                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
