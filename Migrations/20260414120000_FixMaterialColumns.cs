using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class FixMaterialColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename PascalCase column to snake_case to match the rest of the schema
            migrationBuilder.RenameColumn(
                name: "BaseRate",
                table: "materials",
                newName: "base_rate");

            // Drop HarmfulnessFactor — the tariff (base_rate / T_n) already encodes
            // harmfulness per substance group per the legal methodology (Order No. 471).
            // A separate multiplier is not part of the formula Z_n = T_n * I_n * Q_n * K_kat.
            migrationBuilder.DropColumn(
                name: "HarmfulnessFactor",
                table: "materials");

            // Allow NULL so materials without a defined tariff don't default to 0
            migrationBuilder.AlterColumn<decimal>(
                name: "base_rate",
                table: "materials",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "base_rate",
                table: "materials",
                newName: "BaseRate");

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseRate",
                table: "materials",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HarmfulnessFactor",
                table: "materials",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
