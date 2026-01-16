using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Infrastructure.Persistence;
using ReservationSystem.Infrastructure.Services;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly MongoContext _context;
    private readonly ReservationService _reservationService;

    public EventsController(MongoContext context, ReservationService reservationService)
    {
        _context = context;
        _reservationService = reservationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Event @event)
    {
        @event.AvailableCapacity = @event.Capacity; // Başlangıçta hepsi boş
        await _context.Events.InsertOneAsync(@event);
        return Ok(@event);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetails(string id)
    {
        var details = await _reservationService.GetEventDetailsAsync(id);
        return details == null ? NotFound() : Ok(details);
    }
}