using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalMassVolumeToObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "total_mass",
                table: "objects",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_volume",
                table: "objects",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "total_mass",
                table: "objects");

            migrationBuilder.DropColumn(
                name: "total_volume",
                table: "objects");
        }
    }
}
