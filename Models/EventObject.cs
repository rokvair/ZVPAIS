using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    [Table("event_objects")]
    public class EventObject
    {
        [Key]
        [Column("id_event_object")]
        public int IdEventObject { get; set; }

        [Column("event_id")]
        public int EventId { get; set; }

        [Column("object_id")]
        public int ObjectId { get; set; }

        // "water", "soil", "air" — where the leaked material ends up
        [Column("component_type")]
        [StringLength(50)]
        public string? ComponentType { get; set; }

        // K_kat — sensitivity coefficient of the affected area for this event-object link
        [Column("k_kat")]
        public decimal? KKat { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }

        [ForeignKey(nameof(ObjectId))]
        public EnvironmentObject Object { get; set; }
    }
}
