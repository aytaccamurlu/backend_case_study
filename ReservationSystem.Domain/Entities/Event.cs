using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReservationSystem.Domain.Entities
{
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int Capacity { get; set; }

        // Mevcut kullanılabilir kapasite (Hold ve Confirmed düştükten sonra kalan)
        public int AvailableCapacity { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}