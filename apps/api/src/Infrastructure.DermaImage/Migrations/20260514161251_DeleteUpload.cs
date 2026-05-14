using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class DeleteUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "Images",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "Images",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Approved");
        }
    }
}
