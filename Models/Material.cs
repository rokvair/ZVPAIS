using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    [Table("materials")]
    public class Material
    {
        [Key]
        [Column("id_material")]
        public int IdMaterial { get; set; }

        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("toxicity_factor")]
        public double? ToxicityFactor { get; set; }

        [Column("unit")]
        [StringLength(50)]
        public string Unit { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // T_n: tariff per tonne (EUR/t) from Order No. 471 Table 1 or Table 3.
        // The tariff already encodes the harmfulness of the substance group.
        [Column("base_rate")]
        public decimal? BaseRate { get; set; }

        // Substance type: "standard", "bds7", or "suspended". Triggers the Q^0.8 formula branch for water/soil quantities above 1 tonne.
        [Column("substance_type")]
        [StringLength(50)]
        public string? SubstanceType { get; set; }

        // Emission category used in Gaussian plume calculations. Must match a key in EmissionCompound.Ef (e.g. "polymers", "plastics", "paper", "textile").
        [Column("emission_category")]
        [StringLength(50)]
        public string? EmissionCategory { get; set; }
    }
}
