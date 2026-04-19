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

        // T_n — tariff per tonne (Lt/t or EUR/t) from Methodology Table 1 or Table 3.
        // This already reflects the harmfulness of the substance/group.
        [Column("base_rate")]
        public decimal? BaseRate { get; set; }

        // "standard" | "bds7" | "suspended" — triggers Q^0.8 branch for water/soil >1t
        [Column("substance_type")]
        [StringLength(50)]
        public string? SubstanceType { get; set; }
    }
}
