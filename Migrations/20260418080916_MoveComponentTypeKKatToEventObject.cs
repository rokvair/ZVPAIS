using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class MoveComponentTypeKKatToEventObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_event_objects",
                table: "event_objects");

            migrationBuilder.DropColumn(
                name: "component_type",
                table: "objects");

            migrationBuilder.DropColumn(
                name: "k_kat",
                table: "objects");

            migrationBuilder.DropColumn(
                name: "total_mass",
                table: "objects");

            migrationBuilder.DropColumn(
                name: "total_volume",
                table: "objects");

            migrationBuilder.AddColumn<int>(
                name: "id_event_object",
                table: "event_objects",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "component_type",
                table: "event_objects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "k_kat",
                table: "event_objects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_event_objects",
                table: "event_objects",
                column: "id_event_object");

            migrationBuilder.CreateIndex(
                name: "IX_event_objects_event_id",
                table: "event_objects",
                column: "event_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_event_objects",
                table: "event_objects");

            migrationBuilder.DropIndex(
                name: "IX_event_objects_event_id",
                table: "event_objects");

            migrationBuilder.DropColumn(
                name: "id_event_object",
                table: "event_objects");

            migrationBuilder.DropColumn(
                name: "component_type",
                table: "event_objects");

            migrationBuilder.DropColumn(
                name: "k_kat",
                table: "event_objects");

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
                name: "total_mass",
                table: "objects",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_volume",
                table: "objects",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_event_objects",
                table: "event_objects",
                columns: new[] { "event_id", "object_id" });
        }
    }
}
