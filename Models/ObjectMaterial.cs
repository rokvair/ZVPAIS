using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    [Table("object_materials")]
    public class ObjectMaterial
    {
        [Key]
        [Column("id_object_material")]
        public int IdObjectMaterial { get; set; }

        [Column("object_id")]
        public int ObjectId { get; set; }

        [Column("material_id")]
        public int MaterialId { get; set; }

        [Column("percentage")]
        public double? Percentage { get; set; }

        [Column("mass")]
        public double? Mass { get; set; }

        [Column("volume")]
        public double? Volume { get; set; }

        // Q_n2: quantity recovered or neutralised. Net emitted quantity is Mass minus RecoveredQuantity.
        [Column("recovered_quantity")]
        public double? RecoveredQuantity { get; set; }

        [ForeignKey(nameof(ObjectId))]
        public EnvironmentObject EnvironmentObject { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; }
    }
}
