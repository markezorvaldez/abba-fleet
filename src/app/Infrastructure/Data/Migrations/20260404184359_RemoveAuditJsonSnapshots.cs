using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbbaFleet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAuditJsonSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterJson",
                table: "DriverAuditEntries");

            migrationBuilder.DropColumn(
                name: "BeforeJson",
                table: "DriverAuditEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AfterJson",
                table: "DriverAuditEntries",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeforeJson",
                table: "DriverAuditEntries",
                type: "jsonb",
                nullable: true);
        }
    }
}
