using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEventTypeToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Point>(
                name: "coordinates",
                table: "events",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geometry (point, 4326)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Point>(
                name: "coordinates",
                table: "events",
                type: "geometry (point, 4326)",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geometry");
        }
    }
}
