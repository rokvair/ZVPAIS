using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fractions");

            migrationBuilder.DropTable(
                name: "incident_materials");

            migrationBuilder.DropTable(
                name: "map_materials");

            migrationBuilder.DropTable(
                name: "maps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fractions",
                columns: table => new
                {
                    id_fraction = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    component_material_id = table.Column<int>(type: "integer", nullable: false),
                    fk_material_id_material = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    mass = table.Column<double>(type: "double precision", nullable: true),
                    percentage = table.Column<double>(type: "double precision", nullable: true),
                    volume = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fractions", x => x.id_fraction);
                    table.ForeignKey(
                        name: "FK_fractions_materials_component_material_id",
                        column: x => x.component_material_id,
                        principalTable: "materials",
                        principalColumn: "id_material",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fractions_materials_fk_material_id_material",
                        column: x => x.fk_material_id_material,
                        principalTable: "materials",
                        principalColumn: "id_material",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incident_materials",
                columns: table => new
                {
                    id_incident_material = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_event = table.Column<int>(type: "integer", nullable: false),
                    id_material = table.Column<int>(type: "integer", nullable: false),
                    mass = table.Column<double>(type: "double precision", nullable: true),
                    percentage = table.Column<double>(type: "double precision", nullable: true),
                    volume = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_materials", x => x.id_incident_material);
                    table.ForeignKey(
                        name: "FK_incident_materials_events_id_event",
                        column: x => x.id_event,
                        principalTable: "events",
                        principalColumn: "id_event",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_incident_materials_materials_id_material",
                        column: x => x.id_material,
                        principalTable: "materials",
                        principalColumn: "id_material",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maps",
                columns: table => new
                {
                    id_map = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maps", x => x.id_map);
                });

            migrationBuilder.CreateTable(
                name: "map_materials",
                columns: table => new
                {
                    id_map = table.Column<int>(type: "integer", nullable: false),
                    id_material = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map_materials", x => new { x.id_map, x.id_material });
                    table.ForeignKey(
                        name: "FK_map_materials_maps_id_map",
                        column: x => x.id_map,
                        principalTable: "maps",
                        principalColumn: "id_map",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_map_materials_materials_id_material",
                        column: x => x.id_material,
                        principalTable: "materials",
                        principalColumn: "id_material",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fractions_component_material_id",
                table: "fractions",
                column: "component_material_id");

            migrationBuilder.CreateIndex(
                name: "IX_fractions_fk_material_id_material",
                table: "fractions",
                column: "fk_material_id_material");

            migrationBuilder.CreateIndex(
                name: "IX_incident_materials_id_event",
                table: "incident_materials",
                column: "id_event");

            migrationBuilder.CreateIndex(
                name: "IX_incident_materials_id_material",
                table: "incident_materials",
                column: "id_material");

            migrationBuilder.CreateIndex(
                name: "IX_map_materials_id_material",
                table: "map_materials",
                column: "id_material");
        }
    }
}
