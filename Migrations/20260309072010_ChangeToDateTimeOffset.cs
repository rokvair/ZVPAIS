using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "calculation_methods",
                columns: table => new
                {
                    id_calculation_method = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    assigned_objects = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculation_methods", x => x.id_calculation_method);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id_event = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    event_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    coordinates = table.Column<Point>(type: "geometry (point, 4326)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id_event);
                });

            migrationBuilder.CreateTable(
                name: "maps",
                columns: table => new
                {
                    id_map = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maps", x => x.id_map);
                });

            migrationBuilder.CreateTable(
                name: "materials",
                columns: table => new
                {
                    id_material = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    toxicity_factor = table.Column<double>(type: "double precision", nullable: true),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materials", x => x.id_material);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id_user = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id_user);
                });

            migrationBuilder.CreateTable(
                name: "damage_evaluations",
                columns: table => new
                {
                    id_damage_evaluation = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    zalos_dydis = table.Column<double>(type: "double precision", nullable: true),
                    piniginis_dydis = table.Column<double>(type: "double precision", nullable: true),
                    fk_event = table.Column<int>(type: "integer", nullable: false),
                    fk_calculation_method = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_evaluations", x => x.id_damage_evaluation);
                    table.ForeignKey(
                        name: "FK_damage_evaluations_calculation_methods_fk_calculation_method",
                        column: x => x.fk_calculation_method,
                        principalTable: "calculation_methods",
                        principalColumn: "id_calculation_method");
                    table.ForeignKey(
                        name: "FK_damage_evaluations_events_fk_event",
                        column: x => x.fk_event,
                        principalTable: "events",
                        principalColumn: "id_event",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fractions",
                columns: table => new
                {
                    id_fraction = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_material_id_material = table.Column<int>(type: "integer", nullable: false),
                    component_material_id = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<double>(type: "double precision", nullable: true),
                    mass = table.Column<double>(type: "double precision", nullable: true),
                    volume = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    percentage = table.Column<double>(type: "double precision", nullable: true),
                    mass = table.Column<double>(type: "double precision", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "specialists",
                columns: table => new
                {
                    id_user = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    field_of_expertise = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specialists", x => x.id_user);
                    table.ForeignKey(
                        name: "FK_specialists_users_id_user",
                        column: x => x.id_user,
                        principalTable: "users",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_damage_evaluations_fk_calculation_method",
                table: "damage_evaluations",
                column: "fk_calculation_method");

            migrationBuilder.CreateIndex(
                name: "IX_damage_evaluations_fk_event",
                table: "damage_evaluations",
                column: "fk_event");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "damage_evaluations");

            migrationBuilder.DropTable(
                name: "fractions");

            migrationBuilder.DropTable(
                name: "incident_materials");

            migrationBuilder.DropTable(
                name: "map_materials");

            migrationBuilder.DropTable(
                name: "specialists");

            migrationBuilder.DropTable(
                name: "calculation_methods");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "maps");

            migrationBuilder.DropTable(
                name: "materials");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
