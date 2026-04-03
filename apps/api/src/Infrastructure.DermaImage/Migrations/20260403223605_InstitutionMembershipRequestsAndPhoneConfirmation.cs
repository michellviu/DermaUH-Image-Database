using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class InstitutionMembershipRequestsAndPhoneConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInstitutionResponsible",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleInstitutionId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InstitutionMembershipRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionMembershipRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionMembershipRequests_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionMembershipRequests_Users_ApplicantUserId",
                        column: x => x.ApplicantUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstitutionMembershipRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ResponsibleInstitutionId",
                table: "Users",
                column: "ResponsibleInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionMembershipRequests_ApplicantUserId_InstitutionId",
                table: "InstitutionMembershipRequests",
                columns: new[] { "ApplicantUserId", "InstitutionId" },
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionMembershipRequests_InstitutionId",
                table: "InstitutionMembershipRequests",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionMembershipRequests_ReviewedByUserId",
                table: "InstitutionMembershipRequests",
                column: "ReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Institutions_ResponsibleInstitutionId",
                table: "Users",
                column: "ResponsibleInstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Institutions_ResponsibleInstitutionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "InstitutionMembershipRequests");

            migrationBuilder.DropIndex(
                name: "IX_Users_ResponsibleInstitutionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsInstitutionResponsible",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResponsibleInstitutionId",
                table: "Users");
        }
    }
}
