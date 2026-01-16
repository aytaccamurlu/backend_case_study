using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Application.DTOs;
using ReservationSystem.Infrastructure.Services;

namespace ReservationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost("hold")]
    public async Task<IActionResult> Hold([FromBody] CreateReservationRequest request)
    {
        try 
        {
            var reservationId = await _reservationService.CreateHoldAsync(request);
            return Ok(new { ReservationId = reservationId, Message = "5 dakikanız başladı!" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost("confirm/{id}")]
public async Task<IActionResult> Confirm(string id)
{
    try 
    {
        await _reservationService.ConfirmReservationAsync(id);
        return Ok(new { Message = "Rezervasyonunuz başarıyla onaylandı!" });
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
}