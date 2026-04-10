using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ŽVPAIS_API.Models;

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

    // Navigaciniai ryšiai
    [ForeignKey(nameof(ObjectId))]
    public EnvironmentObject EnvironmentObject { get; set; }

    [ForeignKey(nameof(MaterialId))]
    public Material Material { get; set; }
}