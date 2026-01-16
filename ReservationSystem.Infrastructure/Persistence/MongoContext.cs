using MongoDB.Driver;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Infrastructure.Persistence;

public class MongoContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public MongoContext(string connectionString)
    {
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase("ReservationSystemDB");
    }

    public IMongoCollection<Event> Events => _database.GetCollection<Event>("Events");
    public IMongoCollection<Reservation> Reservations => _database.GetCollection<Reservation>("Reservations");
    
    // Transaction başlatmak için Client'a erişim gerekecek
    public IMongoClient Client => _client;
}