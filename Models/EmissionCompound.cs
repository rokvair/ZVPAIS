using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ŽVPAIS_API.Models
{
    [Table("emission_compounds")]
    public class EmissionCompound
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [StringLength(500)]
        public string Name { get; set; } = "";

        // Order No. 471 base rate (EUR/t), used in the damage formula.
        [Column("base_rate")]
        public double? BaseRate { get; set; }

        // JSONB map of emission factors: category to kg of this compound emitted per tonne of material burned.
        [Column("ef", TypeName = "jsonb")]
        public string EfJson { get; set; } = "{}";

        [NotMapped]
        public Dictionary<string, double> Ef
        {
            get => JsonSerializer.Deserialize<Dictionary<string, double>>(EfJson) ?? new();
            set => EfJson = JsonSerializer.Serialize(value);
        }
    }
}
