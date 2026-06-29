using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstitutionCountry",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InstitutionDescription",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InstitutionName",
                table: "Images");

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
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_InstitutionId",
                table: "Images",
                column: "InstitutionId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Institutions_InstitutionId",
                table: "Images");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.DropIndex(
                name: "IX_Images_InstitutionId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Images");

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
        }
    }
}
