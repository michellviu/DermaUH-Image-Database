using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class EliminarRelacionContribuidorEnImagenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_ContributorId",
                table: "Images");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Images_Users_ContributorId",
                table: "Images",
                column: "ContributorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
