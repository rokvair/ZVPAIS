using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    [Table("objects")]
    public class EnvironmentObject
    {
        [Key]
        [Column("id_object")]
        public int IdObject { get; set; }

        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("total_mass")]
        public double? TotalMass { get; set; }

        [Column("total_volume")]
        public double? TotalVolume { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ObjectMaterial> ObjectMaterials { get; set; }
        public ICollection<EventObject> EventObjects { get; set; }
    }
}
