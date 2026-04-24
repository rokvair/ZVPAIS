using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    public partial class SeedCombustibleMaterials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Polymers: emission factor data available under 'polymers' key
            migrationBuilder.Sql(@"
                INSERT INTO materials (name, description, unit, substance_type, base_rate, toxicity_factor, emission_category, created_at)
                VALUES
                ('PVC (polivinilchloridas)',   'Polivinilchloridas — plastmas sudedamoji dalis',          't', 'standard', NULL, NULL, 'polymers', NOW()),
                ('Polistirenas (PS)',          'Polistirenas — putplastis ir kietasis',                   't', 'standard', NULL, NULL, 'polymers', NOW()),
                ('Poliuretanas (PU)',          'Poliuretano putos — izoliacijos, baldai',                 't', 'standard', NULL, NULL, 'polymers', NOW()),
                ('Nailonas / Poliamidas (PA)', 'Poliamidai: nailonas ir kiti sintetiniai polimerai',      't', 'standard', NULL, NULL, 'polymers', NOW()),
                ('Guma / Kaučiukas',          'Vulkanizuota guma, padangos ir techniniai gaminiai',      't', 'standard', NULL, NULL, 'polymers', NOW());
            ");

            // Plastics: emission factor data available under 'plastics' key
            migrationBuilder.Sql(@"
                INSERT INTO materials (name, description, unit, substance_type, base_rate, toxicity_factor, emission_category, created_at)
                VALUES
                ('Polietilenas (PE)',          'Mažo ir aukšto tankio polietilenas — pakuotė, vamzdžiai', 't', 'standard', NULL, NULL, 'plastics', NOW()),
                ('Polipropilenas (PP)',        'Polipropilenas — tara, automobilių detalės',              't', 'standard', NULL, NULL, 'plastics', NOW()),
                ('Mišrūs plastikas',          'Mišri plastikų frakcija (PE, PP, PS, PET ir kt.)',        't', 'standard', NULL, NULL, 'plastics', NOW()),
                ('PET (polietilentereftalatas)','PET — buteliai, pakuotė, sintetiniai pluoštai',         't', 'standard', NULL, NULL, 'plastics', NOW());
            ");

            // Paper: emission factor data available under 'paper' key
            migrationBuilder.Sql(@"
                INSERT INTO materials (name, description, unit, substance_type, base_rate, toxicity_factor, emission_category, created_at)
                VALUES
                ('Popierius',                 'Popieriaus produktai: lapai, knygos, spaudiniai',         't', 'standard', NULL, NULL, 'paper', NOW()),
                ('Kartonas',                  'Gofruotas ir kietas kartonas, pakavimo medžiagos',        't', 'standard', NULL, NULL, 'paper', NOW()),
                ('Popierius ir kartonas (mišrus)', 'Mišri popieriaus ir kartono frakcija',              't', 'standard', NULL, NULL, 'paper', NOW());
            ");

            // Textile: emission factor data available under 'textile' key
            migrationBuilder.Sql(@"
                INSERT INTO materials (name, description, unit, substance_type, base_rate, toxicity_factor, emission_category, created_at)
                VALUES
                ('Natūrali tekstilė (medvilnė, vilna)', 'Medvilnė, linas, vilna ir kiti natūralūs pluoštai', 't', 'standard', NULL, NULL, 'textile', NOW()),
                ('Sintetinė tekstilė',        'Sintetiniai audiniai: poliesteris, akrilai, nailonas',    't', 'standard', NULL, NULL, 'textile', NOW()),
                ('Drabužiai (mišrūs)',         'Mišri drabužių ir tekstilės frakcija',                   't', 'standard', NULL, NULL, 'textile', NOW());
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM materials WHERE name IN (
                    'PVC (polivinilchloridas)',
                    'Polistirenas (PS)',
                    'Poliuretanas (PU)',
                    'Nailonas / Poliamidas (PA)',
                    'Guma / Kaučiukas',
                    'Polietilenas (PE)',
                    'Polipropilenas (PP)',
                    'Mišrūs plastikas',
                    'PET (polietilentereftalatas)',
                    'Popierius',
                    'Kartonas',
                    'Popierius ir kartonas (mišrus)',
                    'Natūrali tekstilė (medvilnė, vilna)',
                    'Sintetinė tekstilė',
                    'Drabužiai (mišrūs)'
                );
            ");
        }
    }
}
