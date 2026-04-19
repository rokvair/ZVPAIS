using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCalculationMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_damage_evaluations_calculation_methods_fk_calculation_method",
                table: "damage_evaluations");

            migrationBuilder.DropTable(
                name: "calculation_methods");

            migrationBuilder.DropIndex(
                name: "IX_damage_evaluations_fk_calculation_method",
                table: "damage_evaluations");

            migrationBuilder.DropColumn(
                name: "fk_calculation_method",
                table: "damage_evaluations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "fk_calculation_method",
                table: "damage_evaluations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "calculation_methods",
                columns: table => new
                {
                    id_calculation_method = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    assigned_objects = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculation_methods", x => x.id_calculation_method);
                });

            migrationBuilder.CreateIndex(
                name: "IX_damage_evaluations_fk_calculation_method",
                table: "damage_evaluations",
                column: "fk_calculation_method");

            migrationBuilder.AddForeignKey(
                name: "FK_damage_evaluations_calculation_methods_fk_calculation_method",
                table: "damage_evaluations",
                column: "fk_calculation_method",
                principalTable: "calculation_methods",
                principalColumn: "id_calculation_method");
        }
    }
}
