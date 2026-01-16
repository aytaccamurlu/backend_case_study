using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReservationSystem.Domain.Entities
{
    public enum ReservationStatus
    {
        Hold = 1,
        Confirmed = 2,
        Expired = 3
    }

    public class Reservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string EventId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public ReservationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }
    }
}