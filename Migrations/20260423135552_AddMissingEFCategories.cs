using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingEFCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add emission factors (kg/tonne = g/kg burned) for categories missing EF data.
            // Sources: EPA AP-42 §13.1 (wood/biomass), §5.1 (petroleum),
            //          SFPE Handbook Ch.62 (rubber/smoke), IPCC Vol.5 (carbon).
            migrationBuilder.Sql(@"
                UPDATE emission_compounds SET ef = ef || '{""wood"":60.0,""oil"":35.0,""rubber"":30.0,""liquid_fuel"":40.0,""carbon"":30.0,""halogenated"":20.0,""liquid_organic"":20.0}'::jsonb WHERE id = 81;
                UPDATE emission_compounds SET ef = ef || '{""wood"":2.5,""oil"":6.0,""rubber"":3.0,""liquid_fuel"":6.0,""carbon"":2.0,""halogenated"":2.0,""liquid_organic"":3.0}'::jsonb WHERE id = 82;
                UPDATE emission_compounds SET ef = ef || '{""wood"":0.02,""oil"":0.05,""rubber"":0.1,""liquid_fuel"":0.01,""carbon"":0.01,""halogenated"":150.0,""liquid_organic"":0.05}'::jsonb WHERE id = 84;
                UPDATE emission_compounds SET ef = ef || '{""wood"":0.1,""oil"":0.1,""rubber"":0.2,""liquid_fuel"":0.1,""carbon"":0.05,""halogenated"":0.05,""liquid_organic"":0.1}'::jsonb WHERE id = 83;
                UPDATE emission_compounds SET ef = ef || '{""wood"":0.3,""oil"":0.5,""rubber"":1.0,""liquid_fuel"":0.8,""carbon"":0.05,""halogenated"":0.2,""liquid_organic"":1.0}'::jsonb WHERE id = 8;
                UPDATE emission_compounds SET ef = ef || '{""wood"":1.5,""oil"":0.3,""rubber"":0.5,""liquid_fuel"":0.3,""carbon"":0.1,""halogenated"":0.2,""liquid_organic"":0.5}'::jsonb WHERE id = 16;
                UPDATE emission_compounds SET ef = ef || '{""wood"":0.5,""oil"":0.1,""rubber"":0.2,""liquid_fuel"":0.1,""liquid_organic"":0.2}'::jsonb WHERE id = 17;
                UPDATE emission_compounds SET ef = ef || '{""wood"":0.3,""oil"":0.5,""rubber"":1.0,""liquid_fuel"":0.5,""carbon"":0.1,""halogenated"":0.3,""liquid_organic"":0.4}'::jsonb WHERE id = 76;
                UPDATE emission_compounds SET ef = ef || '{""wood"":0.001,""oil"":0.001,""rubber"":0.003,""liquid_fuel"":0.001,""halogenated"":1.0,""liquid_organic"":0.001}'::jsonb WHERE id = 89;
                UPDATE emission_compounds SET ef = ef || '{""wood"":0.1,""oil"":0.2,""rubber"":0.3,""liquid_fuel"":0.2,""liquid_organic"":0.1}'::jsonb WHERE id = 92;
                UPDATE emission_compounds SET ef = ef || '{""wood"":0.05,""oil"":0.08,""rubber"":0.15,""liquid_fuel"":0.06,""liquid_organic"":0.04}'::jsonb WHERE id = 102;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO materials (name, description, unit, substance_type, base_rate, toxicity_factor, emission_category, created_at)
                VALUES
                ('Mediena / Pjuvenos',           'Mediena, pjuvenos, skiedros, medienos apdorojimo atliekos',       't', 'standard', NULL, NULL, 'wood',           NOW()),
                ('Nafta / Mineralinė alyva',     'Nafta, mineralinės alyvos, tepalai ir kt. naftos produktai',     't', 'standard', NULL, NULL, 'oil',            NOW()),
                ('Guma (techninis kaučiukas)',    'Techninis kaučiukas, padangos, gumos gaminiai',                  't', 'standard', NULL, NULL, 'rubber',         NOW()),
                ('Dyzelinas / Benzinas',         'Dyzelinas, žibalo frakcija, benzinas ir kiti skystieji degalai', 't', 'standard', NULL, NULL, 'liquid_fuel',    NOW()),
                ('Anglis / Koksas',              'Akmens anglis, koksas, suodžiai, grafitas',                      't', 'standard', NULL, NULL, 'carbon',         NOW()),
                ('Halogenintieji organiniai jk', 'Chlorintieji ir fluorintieji organiniai junginiai',              't', 'standard', NULL, NULL, 'halogenated',    NOW()),
                ('Lakieji organiniai junginiai', 'Degalai, tirpikliai ir kiti lakieji organiniai junginiai',       't', 'standard', NULL, NULL, 'liquid_organic',  NOW());
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE emission_compounds SET ef = ef - 'wood' - 'oil' - 'rubber' - 'liquid_fuel' - 'carbon' - 'halogenated' - 'liquid_organic'
                WHERE id IN (8, 16, 17, 76, 81, 82, 83, 84, 89, 92, 102);

                DELETE FROM materials WHERE name IN (
                    'Mediena / Pjuvenos', 'Nafta / Mineralinė alyva', 'Guma (techninis kaučiukas)',
                    'Dyzelinas / Benzinas', 'Anglis / Koksas', 'Halogenintieji organiniai jk',
                    'Lakieji organiniai junginiai'
                );
            ");
        }
    }
}
