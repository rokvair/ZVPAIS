using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    // I_n: CPI-based indexing coefficient updated quarterly per methodology section 6.
    [Table("indexing_coefficients")]
    public class IndexingCoefficient
    {
        [Key]
        [Column("id_indexing_coefficient")]
        public int IdIndexingCoefficient { get; set; }

        [Column("year")]
        public int Year { get; set; }

        // Quarter of the year, values 1 to 4.
        [Column("quarter")]
        public int Quarter { get; set; }

        [Column("coefficient")]
        public decimal Coefficient { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
