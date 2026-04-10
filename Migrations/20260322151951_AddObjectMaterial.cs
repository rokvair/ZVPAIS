using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddObjectMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "object_materials",
                columns: table => new
                {
                    object_id = table.Column<int>(type: "integer", nullable: false),
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<double>(type: "double precision", nullable: true),
                    mass = table.Column<double>(type: "double precision", nullable: true),
                    volume = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_object_materials", x => new { x.object_id, x.material_id });
                    table.ForeignKey(
                        name: "FK_object_materials_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "id_material",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_object_materials_objects_object_id",
                        column: x => x.object_id,
                        principalTable: "objects",
                        principalColumn: "id_object",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_object_materials_material_id",
                table: "object_materials",
                column: "material_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "object_materials");
        }
    }
}
