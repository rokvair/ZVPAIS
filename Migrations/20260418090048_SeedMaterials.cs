using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ŽVPAIS_API.Migrations
{
    // Seeds materials from Order No. 471 Tables 1 (water/soil) and 3 (air).
    // Base tariffs are original Lt/t values converted to EUR at fixed rate 3.4528 Lt = 1 EUR.
    // I_n indexing coefficient (quarterly CPI vs Aug 2002 base) is applied at runtime.
    public partial class SeedMaterials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Table 1: Water / Soil pollutants
            migrationBuilder.Sql(@"
                INSERT INTO materials (name, description, unit, substance_type, base_rate, toxicity_factor, created_at)
                VALUES
                ('BDS₇ (biocheminis deguonies suvartojimas)', 'Organinės medžiagos pagal biocheminį deguonies suvartojimą', 't', 'bds7',     22155.05, NULL, NOW()),
                ('Suspenduotos medžiagos',                   'Suspenduotos medžiagos vandenyje',                           't', 'suspended', 6719.81,  NULL, NOW()),
                ('Bendras azotas',                           'Azoto junginiai',                                            't', 'standard',  8688.26,  NULL, NOW()),
                ('Bendras fosforas',                         'Fosforo junginiai',                                          't', 'standard',  43441.28, NULL, NOW()),
                ('Sulfatai',                                 'Sulfatų junginiai',                                          't', 'standard',  34.75,    NULL, NOW()),
                ('Chloridai',                                'Chloridų junginiai',                                         't', 'standard',  20.27,    NULL, NOW()),
                ('I grupė (vanduo/žemė)',  'Halogeniniai angliavandeniliai: trichlorbenzolas, heksachlorbenzolas, HCH ir kt.; benzapirenas', 't', 'standard', 503956397, NULL, NOW()),
                ('II grupė (vanduo/žemė)', 'Arsenas, kadmis, gyvsidabris, vanadis, chromas VI, formaldehidas, fenolis, pesticidai',         't', 'standard', 11478534,  NULL, NOW()),
                ('III grupė (vanduo/žemė)','Tetrachloretilenas, švinas, nikelis, varis, naftalinas ir kt.',                                  't', 'standard', 374476,    NULL, NOW()),
                ('IV grupė (vanduo/žemė)', 'Cinkas, cianidai, nafta ir naftos produktai, acetonas, metanolis, detergentai ir kt.',           't', 'standard', 42430,     NULL, NOW()),
                ('V grupė (vanduo/žemė)',  'Fluoridai, sulfidai, geležis, aliuminis, furfurolas ir kiti',                                    't', 'standard', 3330,      NULL, NOW());
            ");

            // Table 3: Air pollutants
            migrationBuilder.Sql(@"
                INSERT INTO materials (name, description, unit, substance_type, base_rate, toxicity_factor, created_at)
                VALUES
                ('SO₂ (sieros dioksidas)',  'Sieros dioksidas',                                                      't', 'standard', 405.45,   NULL, NOW()),
                ('NOₓ (azoto oksidai)',     'Azoto oksidai',                                                         't', 'standard', 767.42,   NULL, NOW()),
                ('Vanadžio pentoksidas',    'Vanadžio pentoksidas (V₂O₅)',                                           't', 'standard', 1995378,  NULL, NOW()),
                ('Kietosios dalelės',       'Organinės ir neorganinės kietosios dalelės (išskyrus I grupę oras)',     't', 'standard', 240.39,   NULL, NOW()),
                ('I grupė (oras)',  'Labai pavojingi: benzapirenas, berilis, kadmis, gyvsidabris, chromas VI, vinilo chloridas, asbestas ir kt.', 't', 'standard', 315388, NULL, NOW()),
                ('II grupė (oras)', 'Pavojingi: benzolas, chloras, formaldehidas, fenolis, arsenas, akrilonitrilas ir kt.',                      't', 'standard', 16509,  NULL, NOW()),
                ('III grupė (oras)','Vidutiniškai pavojingi: ksilolas, toluenas, acetatai, cinkas, manganas, stirolas ir kt.',                   't', 'standard', 1285,   NULL, NOW()),
                ('IV grupė (oras)', 'Mažai pavojingi: amoniakas, acetonas, anglies monoksidas, etanolis, kalcio oksidas ir kt.',                 't', 'standard', 28.38,  NULL, NOW());
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM materials WHERE name IN (
                    'BDS₇ (biocheminis deguonies suvartojimas)',
                    'Suspenduotos medžiagos',
                    'Bendras azotas',
                    'Bendras fosforas',
                    'Sulfatai',
                    'Chloridai',
                    'I grupė (vanduo/žemė)',
                    'II grupė (vanduo/žemė)',
                    'III grupė (vanduo/žemė)',
                    'IV grupė (vanduo/žemė)',
                    'V grupė (vanduo/žemė)',
                    'SO₂ (sieros dioksidas)',
                    'NOₓ (azoto oksidai)',
                    'Vanadžio pentoksidas',
                    'Kietosios dalelės',
                    'I grupė (oras)',
                    'II grupė (oras)',
                    'III grupė (oras)',
                    'IV grupė (oras)'
                );
            ");
        }
    }
}
