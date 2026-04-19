using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    [Table("events")]
    public class Event
    {
        [Key]
        [Column("id_event")]
        public int IdEvent { get; set; }

        [Column("event_type")]
        public string EventType { get; set; } // 'gaisas', 'medžiagų išsiliejimas', 'stichija'

        [Column("event_date")]
        public DateTimeOffset EventDate { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("coordinates")]
        public Polygon Coordinates { get; set; } // PostGIS taškas

        [Column("status")]
        public string Status { get; set; }
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }
        public ICollection<EventObject> EventObjects { get; set; }
        // Navigaciniai ryšiai
        public ICollection<DamageEvaluation> DamageEvaluations { get; set; }
    }
}