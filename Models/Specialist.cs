using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Models
{
    [Table("specialists")]
    public class Specialist
    {
        [Key]
        [Column("id_user")]
        public int IdUser { get; set; }

        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("field_of_expertise")]
        [StringLength(255)]
        public string FieldOfExpertise { get; set; }

        // Ryšys su User
        [ForeignKey(nameof(IdUser))]
        public User User { get; set; }
    }
}