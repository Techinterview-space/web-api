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
    [Migration("20240229150556_SalaryAgeProperty")]
    partial class SalaryAgeProperty
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

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

            modelBuilder.Entity("Domain.Entities.Salaries.Profession", b =>
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

                    b.ToTable("Professions", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            HexColor = "#C00C50",
                            Title = "Developer"
                        },
                        new
                        {
                            Id = 2L,
                            HexColor = "#C00C50",
                            Title = "QualityAssurance"
                        },
                        new
                        {
                            Id = 3L,
                            HexColor = "#C00C50",
                            Title = "Tester"
                        },
                        new
                        {
                            Id = 4L,
                            HexColor = "#C00C50",
                            Title = "BusinessAnalyst"
                        },
                        new
                        {
                            Id = 5L,
                            HexColor = "#C00C50",
                            Title = "ProjectManager"
                        },
                        new
                        {
                            Id = 6L,
                            HexColor = "#C00C50",
                            Title = "ScrumMaster"
                        },
                        new
                        {
                            Id = 7L,
                            HexColor = "#C00C50",
                            Title = "DevOps"
                        },
                        new
                        {
                            Id = 8L,
                            HexColor = "#C00C50",
                            Title = "SystemAdministrator"
                        },
                        new
                        {
                            Id = 9L,
                            HexColor = "#C00C50",
                            Title = "ProductOwner"
                        },
                        new
                        {
                            Id = 10L,
                            HexColor = "#C00C50",
                            Title = "TeamLeader"
                        },
                        new
                        {
                            Id = 11L,
                            HexColor = "#C00C50",
                            Title = "Architect"
                        },
                        new
                        {
                            Id = 12L,
                            HexColor = "#C00C50",
                            Title = "DataScientist"
                        },
                        new
                        {
                            Id = 13L,
                            HexColor = "#C00C50",
                            Title = "DataAnalyst"
                        },
                        new
                        {
                            Id = 14L,
                            HexColor = "#C00C50",
                            Title = "DataEngineer"
                        },
                        new
                        {
                            Id = 15L,
                            HexColor = "#C00C50",
                            Title = "DataWarehouseSpecialist"
                        },
                        new
                        {
                            Id = 16L,
                            HexColor = "#C00C50",
                            Title = "DatabaseAdministrator"
                        },
                        new
                        {
                            Id = 17L,
                            HexColor = "#C00C50",
                            Title = "TechLeader"
                        },
                        new
                        {
                            Id = 18L,
                            HexColor = "#C00C50",
                            Title = "SystemAnalyst"
                        },
                        new
                        {
                            Id = 19L,
                            HexColor = "#C00C50",
                            Title = "ItHr"
                        },
                        new
                        {
                            Id = 20L,
                            HexColor = "#C00C50",
                            Title = "ItRecruiter"
                        },
                        new
                        {
                            Id = 21L,
                            HexColor = "#C00C50",
                            Title = "UiDesigner"
                        },
                        new
                        {
                            Id = 22L,
                            HexColor = "#C00C50",
                            Title = "UxDesigner"
                        },
                        new
                        {
                            Id = 23L,
                            HexColor = "#C00C50",
                            Title = "UiUxDesigner"
                        },
                        new
                        {
                            Id = 24L,
                            HexColor = "#C00C50",
                            Title = "ProductAnalyst"
                        },
                        new
                        {
                            Id = 25L,
                            HexColor = "#C00C50",
                            Title = "ProductManager"
                        },
                        new
                        {
                            Id = 26L,
                            HexColor = "#C00C50",
                            Title = "ProductDesigner"
                        },
                        new
                        {
                            Id = 27L,
                            HexColor = "#C00C50",
                            Title = "HrNonIt"
                        },
                        new
                        {
                            Id = 28L,
                            HexColor = "#C00C50",
                            Title = "OneCDeveloper"
                        },
                        new
                        {
                            Id = 29L,
                            HexColor = "#C00C50",
                            Title = "ThreeDModeler"
                        },
                        new
                        {
                            Id = 30L,
                            HexColor = "#C00C50",
                            Title = "AndroidDeveloper"
                        },
                        new
                        {
                            Id = 31L,
                            HexColor = "#C00C50",
                            Title = "IosDeveloper"
                        },
                        new
                        {
                            Id = 32L,
                            HexColor = "#C00C50",
                            Title = "MobileDeveloper"
                        },
                        new
                        {
                            Id = 33L,
                            HexColor = "#C00C50",
                            Title = "FrontendDeveloper"
                        },
                        new
                        {
                            Id = 34L,
                            HexColor = "#C00C50",
                            Title = "BackendDeveloper"
                        },
                        new
                        {
                            Id = 35L,
                            HexColor = "#C00C50",
                            Title = "FullstackDeveloper"
                        },
                        new
                        {
                            Id = 36L,
                            HexColor = "#C00C50",
                            Title = "GameDeveloper"
                        },
                        new
                        {
                            Id = 37L,
                            HexColor = "#C00C50",
                            Title = "EmbeddedDeveloper"
                        },
                        new
                        {
                            Id = 38L,
                            HexColor = "#C00C50",
                            Title = "MachineLearningDeveloper"
                        },
                        new
                        {
                            Id = 39L,
                            HexColor = "#C00C50",
                            Title = "Pentester"
                        },
                        new
                        {
                            Id = 40L,
                            HexColor = "#C00C50",
                            Title = "SecurityEngineer"
                        },
                        new
                        {
                            Id = 41L,
                            HexColor = "#C00C50",
                            Title = "SecurityAnalyst"
                        },
                        new
                        {
                            Id = 42L,
                            HexColor = "#C00C50",
                            Title = "TechnicalWriter"
                        },
                        new
                        {
                            Id = 43L,
                            HexColor = "#C00C50",
                            Title = "BiDeveloper"
                        },
                        new
                        {
                            Id = 44L,
                            HexColor = "#C00C50",
                            Title = "ChiefTechnicalOfficer"
                        },
                        new
                        {
                            Id = 45L,
                            HexColor = "#C00C50",
                            Title = "ChiefExecutiveOfficer"
                        },
                        new
                        {
                            Id = 46L,
                            HexColor = "#C00C50",
                            Title = "HeadOfDepartment"
                        },
                        new
                        {
                            Id = 47L,
                            HexColor = "#C00C50",
                            Title = "DeliveryManager"
                        },
                        new
                        {
                            Id = 48L,
                            HexColor = "#C00C50",
                            Title = "Copywriter"
                        },
                        new
                        {
                            Id = 49L,
                            HexColor = "#C00C50",
                            Title = "GameDesigner"
                        },
                        new
                        {
                            Id = 50L,
                            HexColor = "#C00C50",
                            Title = "SecOps"
                        },
                        new
                        {
                            Id = 51L,
                            HexColor = "#C00C50",
                            Title = "TalentAcquisition"
                        },
                        new
                        {
                            Id = 52L,
                            HexColor = "#C00C50",
                            Title = "CustomerSupport"
                        },
                        new
                        {
                            Id = 53L,
                            HexColor = "#C00C50",
                            Title = "TechnicalSupport"
                        });
                });

            modelBuilder.Entity("Domain.Entities.Salaries.Skill", b =>
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

                    b.ToTable("Skills", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Salaries.UserSalary", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int?>("Age")
                        .HasColumnType("integer");

                    b.Property<long?>("City")
                        .HasColumnType("bigint");

                    b.Property<int>("Company")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Currency")
                        .HasColumnType("integer");

                    b.Property<int?>("Gender")
                        .HasColumnType("integer");

                    b.Property<long?>("Grade")
                        .HasColumnType("bigint");

                    b.Property<int>("ProfessionEnum")
                        .HasColumnType("integer");

                    b.Property<long?>("ProfessionId")
                        .HasColumnType("bigint");

                    b.Property<int>("Quarter")
                        .HasColumnType("integer");

                    b.Property<long?>("SkillId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("UseInStats")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.Property<double>("Value")
                        .HasColumnType("double precision");

                    b.Property<long?>("WorkIndustryId")
                        .HasColumnType("bigint");

                    b.Property<int>("Year")
                        .HasColumnType("integer");

                    b.Property<int?>("YearOfStartingWork")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProfessionId");

                    b.HasIndex("SkillId");

                    b.HasIndex("UserId");

                    b.HasIndex("WorkIndustryId");

                    b.ToTable("UserSalaries", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Salaries.WorkIndustry", b =>
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

                    b.ToTable("WorkIndustries", (string)null);
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

                    b.Property<string>("IdentityId")
                        .HasColumnType("text");

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

            modelBuilder.Entity("Domain.Entities.Salaries.Profession", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("Domain.Entities.Salaries.Skill", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("Domain.Entities.Salaries.UserSalary", b =>
                {
                    b.HasOne("Domain.Entities.Salaries.Profession", "Profession")
                        .WithMany("Salaries")
                        .HasForeignKey("ProfessionId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Domain.Entities.Salaries.Skill", "Skill")
                        .WithMany("Salaries")
                        .HasForeignKey("SkillId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Domain.Entities.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Domain.Entities.Salaries.WorkIndustry", "WorkIndustry")
                        .WithMany("Salaries")
                        .HasForeignKey("WorkIndustryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Profession");

                    b.Navigation("Skill");

                    b.Navigation("User");

                    b.Navigation("WorkIndustry");
                });

            modelBuilder.Entity("Domain.Entities.Salaries.WorkIndustry", b =>
                {
                    b.HasOne("Domain.Entities.Users.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.Navigation("CreatedBy");
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

            modelBuilder.Entity("Domain.Entities.Salaries.Profession", b =>
                {
                    b.Navigation("Salaries");
                });

            modelBuilder.Entity("Domain.Entities.Salaries.Skill", b =>
                {
                    b.Navigation("Salaries");
                });

            modelBuilder.Entity("Domain.Entities.Salaries.WorkIndustry", b =>
                {
                    b.Navigation("Salaries");
                });

            modelBuilder.Entity("Domain.Entities.Users.User", b =>
                {
                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
