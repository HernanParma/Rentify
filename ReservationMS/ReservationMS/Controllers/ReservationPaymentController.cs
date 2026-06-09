using Application.Dtos.Request;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ReservationMS.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationPaymentController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationPaymentController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost("{reservationId:guid}/payment")]
    public async Task<IActionResult> ConfirmPayment(Guid reservationId, [FromBody] PaymentConfirmationRequestDto request)
    {
        var reservation = await _reservationService.ConfirmPaymentAsync(reservationId, request);
        if (reservation == null)
            return NotFound();

        return Ok(reservation);
    }
}
