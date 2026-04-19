using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddFormulaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sensitivity_factor",
                table: "events");

            migrationBuilder.AddColumn<string>(
                name: "component_type",
                table: "objects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "k_kat",
                table: "objects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "recovered_quantity",
                table: "object_materials",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "substance_type",
                table: "materials",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "indexing_coefficients",
                columns: table => new
                {
                    id_indexing_coefficient = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    year = table.Column<int>(type: "integer", nullable: false),
                    quarter = table.Column<int>(type: "integer", nullable: false),
                    coefficient = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indexing_coefficients", x => x.id_indexing_coefficient);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "indexing_coefficients");

            migrationBuilder.DropColumn(
                name: "component_type",
                table: "objects");

            migrationBuilder.DropColumn(
                name: "k_kat",
                table: "objects");

            migrationBuilder.DropColumn(
                name: "recovered_quantity",
                table: "object_materials");

            migrationBuilder.DropColumn(
                name: "substance_type",
                table: "materials");

            migrationBuilder.AddColumn<decimal>(
                name: "sensitivity_factor",
                table: "events",
                type: "numeric",
                nullable: true);
        }
    }
}
