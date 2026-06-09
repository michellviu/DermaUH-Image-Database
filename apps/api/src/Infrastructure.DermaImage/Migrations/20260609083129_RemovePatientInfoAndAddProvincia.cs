using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class RemovePatientInfoAndAddProvincia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClinicalHistoryNumber",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "PatientName",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Images",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "ClinicalHistoryNumber",
                table: "Images",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                table: "Images",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
