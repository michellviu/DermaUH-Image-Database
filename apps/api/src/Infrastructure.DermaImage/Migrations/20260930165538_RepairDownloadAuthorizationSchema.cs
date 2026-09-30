using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DermaImage.Migrations
{
    /// <summary>
    /// 20260609080950_RemoveDownloadAuthorization was committed with an empty Up(),
    /// replacing an earlier version that dropped "DownloadAuthorizations" and added
    /// "Users"."IsDownloadAuthorized". Databases created from scratch therefore
    /// lack the column the model requires. The statements are idempotent so this
    /// is a no-op on databases that already applied the original migration.
    /// </summary>
    public partial class RepairDownloadAuthorizationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"DownloadAuthorizations\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"IsDownloadAuthorized\" boolean NOT NULL DEFAULT FALSE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: the column belongs to the current model and
            // must not be removed when rolling back this repair.
        }
    }
}
