using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewerImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Images",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_ReviewedByUserId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ReviewedByUserId",
                table: "Images");

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
    }
}
