using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class ResetSequences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // waste_types and emission_compounds were seeded with explicit IDs,
            // so their IDENTITY sequences were never advanced. Reset them to MAX(id).
            migrationBuilder.Sql(@"
                SELECT setval('materials_id_material_seq',      COALESCE((SELECT MAX(id_material)           FROM materials),            1), (SELECT COUNT(*) > 0 FROM materials));
                SELECT setval('waste_types_id_seq',             COALESCE((SELECT MAX(id)                   FROM waste_types),           1), (SELECT COUNT(*) > 0 FROM waste_types));
                SELECT setval('emission_compounds_id_seq',      COALESCE((SELECT MAX(id)                   FROM emission_compounds),    1), (SELECT COUNT(*) > 0 FROM emission_compounds));
                SELECT setval('events_id_event_seq',            COALESCE((SELECT MAX(id_event)             FROM events),               1), (SELECT COUNT(*) > 0 FROM events));
                SELECT setval('objects_id_object_seq',          COALESCE((SELECT MAX(id_object)            FROM objects),              1), (SELECT COUNT(*) > 0 FROM objects));
                SELECT setval('event_objects_id_event_object_seq', COALESCE((SELECT MAX(id_event_object)  FROM event_objects),         1), (SELECT COUNT(*) > 0 FROM event_objects));
                SELECT setval('indexing_coefficients_id_indexing_coefficient_seq', COALESCE((SELECT MAX(id_indexing_coefficient) FROM indexing_coefficients), 1), (SELECT COUNT(*) > 0 FROM indexing_coefficients));
                SELECT setval('damage_evaluations_id_damage_evaluation_seq', COALESCE((SELECT MAX(id_damage_evaluation) FROM damage_evaluations), 1), (SELECT COUNT(*) > 0 FROM damage_evaluations));
                SELECT setval('users_id_user_seq',              COALESCE((SELECT MAX(id_user)              FROM users),                1), (SELECT COUNT(*) > 0 FROM users));
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sequence positions are not reversible in a meaningful way.
        }
    }
}
