using MongoDB.Driver;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Infrastructure.Persistence;
using ReservationSystem.Application.DTOs;
using Hangfire;

namespace ReservationSystem.Infrastructure.Services;

public class ReservationService
{
    private readonly MongoContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ReservationService(MongoContext context, IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<object?> GetEventDetailsAsync(string eventId)
    {
        var @event = await _context.Events.Find(e => e.Id == eventId).FirstOrDefaultAsync();
        
        if (@event == null) return null;

        var holdCount = await _context.Reservations.CountDocumentsAsync(r => r.EventId == eventId && r.Status == ReservationStatus.Hold);
        var confirmedCount = await _context.Reservations.CountDocumentsAsync(r => r.EventId == eventId && r.Status == ReservationStatus.Confirmed);

        return new
        {
            @event.Title,
            @event.Capacity,
            @event.AvailableCapacity,
            HoldCount = holdCount,
            ConfirmedCount = confirmedCount,
            @event.StartDate,
            @event.EndDate
        };
    }

    public async Task ConfirmReservationAsync(string reservationId)
    {
        var reservation = await _context.Reservations
            .Find(r => r.Id == reservationId && r.Status == ReservationStatus.Hold)
            .FirstOrDefaultAsync();

        if (reservation == null)
            throw new Exception("Rezervasyon bulunamadı veya süresi dolmuş.");

        var result = await _context.Reservations.UpdateOneAsync(
            r => r.Id == reservationId && r.Status == ReservationStatus.Hold,
            Builders<Reservation>.Update.Set(r => r.Status, ReservationStatus.Confirmed)
        );

        if (result.ModifiedCount == 0)
            throw new Exception("Onaylama işlemi başarısız. Rezervasyon süresi dolmuş olabilir.");
    }

    public async Task<string> CreateHoldAsync(CreateReservationRequest request)
    {
        // 1. ADIM: Atomik Kapasite Düşürme
        var eventFilter = Builders<Event>.Filter.And(
            Builders<Event>.Filter.Eq(e => e.Id, request.EventId),
            Builders<Event>.Filter.Eq(e => e.IsActive, true),
            Builders<Event>.Filter.Gt(e => e.AvailableCapacity, 0)
        );

        var update = Builders<Event>.Update.Inc(e => e.AvailableCapacity, -1);
        var updatedEvent = await _context.Events.FindOneAndUpdateAsync(eventFilter, update);

        if (updatedEvent == null)
            throw new Exception("Üzgünüz, kapasite dolu veya etkinlik aktif değil.");

        // 2. ADIM: HOLD Kaydı
        var reservation = new Reservation
        {
            EventId = request.EventId,
            UserId = request.UserId,
            Status = ReservationStatus.Hold,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _context.Reservations.InsertOneAsync(reservation);
        
        // 3. ADIM: Background Job (5 Dakika Sonra Kontrol Et)
        // null check uyarısı için ! kullanıyoruz çünkü InsertOne sonrası ID atanır.
        _backgroundJobClient.Schedule(() => ReleaseHoldIfExpiredAsync(reservation.Id!), TimeSpan.FromMinutes(5));
        
        return reservation.Id!;
    }

    // Hangfire tarafından çağrılacak metod
    public async Task ReleaseHoldIfExpiredAsync(string reservationId)
    {
        // Hala HOLD olan ve süresi dolan kaydı bul
        var reservation = await _context.Reservations
            .Find(r => r.Id == reservationId && r.Status == ReservationStatus.Hold)
            .FirstOrDefaultAsync();

        if (reservation != null)
        {
            // Rezervasyonu EXPIRED yap
            await _context.Reservations.UpdateOneAsync(
                r => r.Id == reservationId,
                Builders<Reservation>.Update.Set(r => r.Status, ReservationStatus.Expired)
            );

            // Kapasiteyi İade Et
            await _context.Events.UpdateOneAsync(
                e => e.Id == reservation.EventId,
                Builders<Event>.Update.Inc(e => e.AvailableCapacity, 1)
            );
        }
    }
}