using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInstitutionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Institutions_InstitutionId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Institutions_InstitutionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "InstitutionJoinRequests");

            migrationBuilder.DropTable(
                name: "InstitutionResponsibles");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.DropIndex(
                name: "IX_Users_InstitutionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Images_InstitutionId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "ClinicalHistoryNumber",
                table: "Images",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DermoscopicComments",
                table: "Images",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HistopathologicalDiagnosis",
                table: "Images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InformedConsent",
                table: "Images",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InformedConsentDate",
                table: "Images",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InformedConsentText",
                table: "Images",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionCountry",
                table: "Images",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionDescription",
                table: "Images",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionName",
                table: "Images",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                table: "Images",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalHistory",
                table: "Images",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkinColor",
                table: "Images",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SunExposure",
                table: "Images",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClinicalHistoryNumber",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "DermoscopicComments",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "HistopathologicalDiagnosis",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InformedConsent",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InformedConsentDate",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InformedConsentText",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InstitutionCountry",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InstitutionDescription",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InstitutionName",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "PatientName",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "PersonalHistory",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "SkinColor",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "SunExposure",
                table: "Images");

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Images",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstitutionJoinRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionJoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionJoinRequests_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionJoinRequests_Users_ApplicantUserId",
                        column: x => x.ApplicantUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionJoinRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InstitutionResponsibles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionResponsibles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionResponsibles_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionResponsibles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstitutionId",
                table: "Users",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_InstitutionId",
                table: "Images",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionJoinRequests_ApplicantUserId_InstitutionId_Status",
                table: "InstitutionJoinRequests",
                columns: new[] { "ApplicantUserId", "InstitutionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionJoinRequests_InstitutionId",
                table: "InstitutionJoinRequests",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionJoinRequests_ReviewedByUserId",
                table: "InstitutionJoinRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionResponsibles_InstitutionId_UserId",
                table: "InstitutionResponsibles",
                columns: new[] { "InstitutionId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionResponsibles_UserId",
                table: "InstitutionResponsibles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_Name",
                table: "Institutions",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Institutions_InstitutionId",
                table: "Images",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Institutions_InstitutionId",
                table: "Users",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
