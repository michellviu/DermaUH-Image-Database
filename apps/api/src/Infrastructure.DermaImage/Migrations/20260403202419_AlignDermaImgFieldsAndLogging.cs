using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class AlignDermaImgFieldsAndLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcquisitionDay",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Attribution",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ConcomitantBiopsy",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "DiagnosisLevel2",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "DiagnosisLevel3",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "DiagnosisLevel4",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "DiagnosisLevel5",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Melanocytic",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "CopyrightLicense",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "InjuryType",
                table: "Images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoType",
                table: "Images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoType",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "InjuryType",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "CopyrightLicense",
                table: "Images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcquisitionDay",
                table: "Images",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attribution",
                table: "Images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConcomitantBiopsy",
                table: "Images",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisLevel2",
                table: "Images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisLevel3",
                table: "Images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisLevel4",
                table: "Images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisLevel5",
                table: "Images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Melanocytic",
                table: "Images",
                type: "boolean",
                nullable: true);
        }
    }
}
