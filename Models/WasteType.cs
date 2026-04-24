using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ŽVPAIS_API.Models
{
    [Table("waste_types")]
    public class WasteType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("ewc_code")]
        [StringLength(20)]
        public string EwcCode { get; set; } = "";

        [Column("description")]
        public string Description { get; set; } = "";

        [Column("is_hazardous")]
        public bool IsHazardous { get; set; }

        [Column("is_combustible")]
        public bool IsCombustible { get; set; }

        [Column("total_mass_tonnes")]
        public double TotalMassTonnes { get; set; }

        // JSONB map of morphology fractions: category to percentage (0-100) of this waste type.
        [Column("morphology", TypeName = "jsonb")]
        public string MorphologyJson { get; set; } = "{}";

        [Column("morph_sum")]
        public double MorphSum { get; set; }

        // JSONB map of binary property flags used for waste classification (e.g. "sausa_liekana", "sunkieji_metalai").
        [Column("flags", TypeName = "jsonb")]
        public string FlagsJson { get; set; } = "{}";

        [NotMapped]
        public Dictionary<string, double> Morphology
        {
            get => JsonSerializer.Deserialize<Dictionary<string, double>>(MorphologyJson) ?? new();
            set => MorphologyJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Dictionary<string, bool> Flags
        {
            get => JsonSerializer.Deserialize<Dictionary<string, bool>>(FlagsJson) ?? new();
            set => FlagsJson = JsonSerializer.Serialize(value);
        }
    }
}
