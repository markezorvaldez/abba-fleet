using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbbaFleet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigratePermissionsToClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "AspNetUserClaims" ("ClaimType", "ClaimValue", "UserId")
                SELECT 'Permission', perm_value, "Id"
                FROM "AspNetUsers",
                     jsonb_array_elements_text("Permissions"::jsonb) AS perm_value
                WHERE "Permissions" <> '[]';
                """);

            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Permissions",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql("""
                UPDATE "AspNetUsers" u
                SET "Permissions" = COALESCE(
                    (SELECT json_agg(c."ClaimValue")::text
                     FROM "AspNetUserClaims" c
                     WHERE c."UserId" = u."Id" AND c."ClaimType" = 'Permission'),
                    '[]');

                DELETE FROM "AspNetUserClaims" WHERE "ClaimType" = 'Permission';
                """);
        }
    }
}
