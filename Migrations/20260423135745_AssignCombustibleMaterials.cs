using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class AssignCombustibleMaterials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Assign realistic combustible materials to seeded environment objects.
            // Uses ON CONFLICT DO NOTHING so re-running the migration is safe.
            migrationBuilder.Sql(@"
                INSERT INTO object_materials (object_id, material_id, mass, volume, percentage, recovered_quantity)
                SELECT o.id_object, m.id_material, vals.mass, NULL, NULL, NULL
                FROM (VALUES
                    -- Naftos produktų saugykla: diesel, oil, PE piping
                    ('Naftos produktu saugykla',    'Dyzelinas / Benzinas',         40.0),
                    ('Naftos produktu saugykla',    'Nafta / Mineralinė alyva',     8.0),
                    ('Naftos produktu saugykla',    'Polietilenas (PE)',             0.5),

                    -- Transformatorinė podstancija: transformer oil
                    ('Transformatorine podstancija','Nafta / Mineralinė alyva',     7.0),

                    -- Cheminių medžiagų sandėlis: packaging, containment
                    ('Cheminiu medziagu sandelis',  'Polietilenas (PE)',             0.8),
                    ('Cheminiu medziagu sandelis',  'Polipropilenas (PP)',           0.5),
                    ('Cheminiu medziagu sandelis',  'PVC (polivinilchloridas)',      0.3),
                    ('Cheminiu medziagu sandelis',  'Kartonas',                      1.2),
                    ('Cheminiu medziagu sandelis',  'Guma (techninis kaučiukas)',    0.2),

                    -- Katilinė: fuel for boiler
                    ('Kateline',                    'Dyzelinas / Benzinas',         5.0),
                    ('Kateline',                    'Anglis / Koksas',              3.0),

                    -- Galvaninis cechas: plastic tanks and containers
                    ('Galvaninis cechas',           'PVC (polivinilchloridas)',      1.5),
                    ('Galvaninis cechas',           'Polipropilenas (PP)',           1.0),

                    -- Naftos vamzdynas: pipe coating/insulation, fuel
                    ('Naftos vamzdynas',            'Nafta / Mineralinė alyva',     25.0),
                    ('Naftos vamzdynas',            'Polietilenas (PE)',             2.0),

                    -- Nuotekų valymo įrenginiai: piping and insulation materials
                    ('Nuoteku valymo irenginiai',   'Polietilenas (PE)',             1.5),
                    ('Nuoteku valymo irenginiai',   'Polipropilenas (PP)',           1.0)
                ) AS vals(obj_name, mat_name, mass)
                JOIN objects o ON o.name = vals.obj_name
                JOIN materials m ON m.name = vals.mat_name
                ON CONFLICT (object_id, material_id) DO NOTHING;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM object_materials
                WHERE (object_id, material_id) IN (
                    SELECT o.id_object, m.id_material
                    FROM (VALUES
                        ('Naftos produktu saugykla',    'Dyzelinas / Benzinas'),
                        ('Naftos produktu saugykla',    'Nafta / Mineralinė alyva'),
                        ('Naftos produktu saugykla',    'Polietilenas (PE)'),
                        ('Transformatorine podstancija','Nafta / Mineralinė alyva'),
                        ('Cheminiu medziagu sandelis',  'Polietilenas (PE)'),
                        ('Cheminiu medziagu sandelis',  'Polipropilenas (PP)'),
                        ('Cheminiu medziagu sandelis',  'PVC (polivinilchloridas)'),
                        ('Cheminiu medziagu sandelis',  'Kartonas'),
                        ('Cheminiu medziagu sandelis',  'Guma (techninis kaučiukas)'),
                        ('Kateline',                    'Dyzelinas / Benzinas'),
                        ('Kateline',                    'Anglis / Koksas'),
                        ('Galvaninis cechas',           'PVC (polivinilchloridas)'),
                        ('Galvaninis cechas',           'Polipropilenas (PP)'),
                        ('Naftos vamzdynas',            'Nafta / Mineralinė alyva'),
                        ('Naftos vamzdynas',            'Polietilenas (PE)'),
                        ('Nuoteku valymo irenginiai',   'Polietilenas (PE)'),
                        ('Nuoteku valymo irenginiai',   'Polipropilenas (PP)')
                    ) AS vals(obj_name, mat_name)
                    JOIN objects o ON o.name = vals.obj_name
                    JOIN materials m ON m.name = vals.mat_name
                );
            ");
        }
    }
}
