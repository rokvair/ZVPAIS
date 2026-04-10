using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddEventObjectTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "objects",
                columns: table => new
                {
                    id_object = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_objects", x => x.id_object);
                });

            migrationBuilder.CreateTable(
                name: "event_objects",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    object_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_objects", x => new { x.event_id, x.object_id });
                    table.ForeignKey(
                        name: "FK_event_objects_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id_event",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_objects_objects_object_id",
                        column: x => x.object_id,
                        principalTable: "objects",
                        principalColumn: "id_object",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_objects_object_id",
                table: "event_objects",
                column: "object_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_objects");

            migrationBuilder.DropTable(
                name: "objects");
        }
    }
}
