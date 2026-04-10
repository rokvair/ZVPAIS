using System.ComponentModel.DataAnnotations.Schema;

namespace ŽVPAIS_API.Models
{
    [Table("event_objects")]
    public class EventObject
    {
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("object_id")]
        public int ObjectId { get; set; }

        // Navigaciniai ryšiai
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }

        [ForeignKey(nameof(ObjectId))]
        public EnvironmentObject Object { get; set; }
    }
}