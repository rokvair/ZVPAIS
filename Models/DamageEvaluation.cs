using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    [Table("damage_evaluations")]
    public class DamageEvaluation
    {
        [Key]
        [Column("id_damage_evaluation")]
        public int IdDamageEvaluation { get; set; }

        [Column("data")]
        public DateTime Data { get; set; } // vertinimo data

        [Column("zalos_dydis")]
        public double? ZalosDydis { get; set; }

        [Column("piniginis_dydis")]
        public double? PiniginisDydis { get; set; }

        [Column("fk_event")]
        public int EventId { get; set; }

        [Column("fk_calculation_method")]
        public int? CalculationMethodId { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigaciniai ryšiai
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }

        [ForeignKey(nameof(CalculationMethodId))]
        public CalculationMethod CalculationMethod { get; set; }
    }
}