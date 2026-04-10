using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id_user")]
        public int IdUser { get; set; }

        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Column("password")]
        [StringLength(255)]
        public string Password { get; set; } // realiai saugomas hash

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Ryšys su specialist (1:1)
        public Specialist Specialist { get; set; }
    }
}