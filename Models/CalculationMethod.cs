using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    [Table("calculation_methods")]
    public class CalculationMethod
    {
        [Key]
        [Column("id_calculation_method")]
        public int IdCalculationMethod { get; set; }

        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("assigned_objects")]
        public string AssignedObjects { get; set; } // laikina, kol bus Objects lentelė

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigaciniai ryšiai
        public ICollection<DamageEvaluation> DamageEvaluations { get; set; }
    }
}