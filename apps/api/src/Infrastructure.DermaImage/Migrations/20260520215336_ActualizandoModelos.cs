using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class ActualizandoModelos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_ReviewedByUserId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ReviewedByUserId",
                table: "Images");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-0002-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-0003-0000-0000-000000000003"));

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ReviewComment",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Images");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Images",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved");

            migrationBuilder.AddColumn<string>(
                name: "ReviewComment",
                table: "Images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Images",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "Images",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0002-0000-0000-000000000002"), "01e3e9e5-e364-44d9-9c43-49699c32cc19", "Contributor", "CONTRIBUTOR" },
                    { new Guid("a1b2c3d4-0003-0000-0000-000000000003"), "fb6c7b75-f57a-4665-9ada-f152f9907452", "Reviewer", "REVIEWER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_ReviewedByUserId",
                table: "Images",
                column: "ReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Users_ReviewedByUserId",
                table: "Images",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
