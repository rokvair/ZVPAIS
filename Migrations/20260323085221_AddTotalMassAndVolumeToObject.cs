using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalMassAndVolumeToObject : Migration
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

            migrationBuilder.AlterColumn<decimal>(
                name: "HarmfulnessFactor",
                table: "materials",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseRate",
                table: "materials",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "notes",
                table: "damage_evaluations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
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

            migrationBuilder.AlterColumn<decimal>(
                name: "HarmfulnessFactor",
                table: "materials",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseRate",
                table: "materials",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "notes",
                table: "damage_evaluations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
