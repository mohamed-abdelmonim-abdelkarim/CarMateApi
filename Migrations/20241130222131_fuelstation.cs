using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMate.Migrations
{
    /// <inheritdoc />
    public partial class fuelstation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FuelStationId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FuelStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelStations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_FuelStationId",
                table: "ServiceRequests",
                column: "FuelStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_FuelStations_FuelStationId",
                table: "ServiceRequests",
                column: "FuelStationId",
                principalTable: "FuelStations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_FuelStations_FuelStationId",
                table: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "FuelStations");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_FuelStationId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "FuelStationId",
                table: "ServiceRequests");
        }
    }
}
