using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ŽVPAIS_API.Models;

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
        public decimal? BaseRate { get; set; }       // bazinis įkainis (Eur/kg, Eur/l)
        public decimal? HarmfulnessFactor { get; set; }

        // Navigaciniai ryšiai
    }
}