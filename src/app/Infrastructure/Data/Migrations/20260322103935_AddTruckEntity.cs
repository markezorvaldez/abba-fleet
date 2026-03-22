using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbbaFleet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTruckEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trucks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlateNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TruckModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnershipType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DateAcquired = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trucks_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId",
                table: "Trucks",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_IsActive",
                table: "Trucks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_PlateNumber",
                table: "Trucks",
                column: "PlateNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trucks");
        }
    }
}
