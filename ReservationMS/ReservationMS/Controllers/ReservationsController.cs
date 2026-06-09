using Application.Dtos.Request;

using Application.Interfaces.IServices;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;



namespace ReservationMS.Controllers;



[ApiController]

[Route("api/v1/[controller]")]

public class ReservationsController : ControllerBase

{

    private readonly IReservationService _reservationService;



    public ReservationsController(IReservationService reservationService)

    {

        _reservationService = reservationService;

    }



    [HttpGet]

    [Authorize(Roles = "Admin,Employee")]

    public async Task<IActionResult> GetAll(

        [FromQuery] int? statusId,

        [FromQuery] int? branchId,

        [FromQuery] int? userId,

        [FromQuery] string? search,

        [FromQuery] DateTime? from,

        [FromQuery] DateTime? to)

    {

        var filter = new ReservationFilterDto

        {

            StatusId = statusId,

            BranchId = branchId,

            UserId = userId,

            Search = search,

            From = from,

            To = to

        };



        var reservations = await _reservationService.GetAllAsync(filter);

        return Ok(reservations);

    }



    [HttpGet("{id:guid}")]

    public async Task<IActionResult> GetById(Guid id)

    {

        var reservation = await _reservationService.GetByIdAsync(id);

        if (reservation == null)

            return NotFound();



        return Ok(reservation);

    }



    [HttpGet("user/{userId:int}")]

    public async Task<IActionResult> GetByUserId(int userId)

    {

        var reservations = await _reservationService.GetByUserIdAsync(userId);

        return Ok(reservations);

    }



    [HttpGet("vehicle/{vehicleId:guid}/booked-ranges")]

    public async Task<IActionResult> GetBookedRangesByVehicle(Guid vehicleId)

    {

        var ranges = await _reservationService.GetBookedRangesByVehicleAsync(vehicleId);

        return Ok(ranges);

    }



    [HttpGet("availability")]

    public async Task<IActionResult> GetAvailability(

        [FromQuery] int branchId,

        [FromQuery] DateTime start,

        [FromQuery] DateTime end)

    {

        try

        {

            var vehicles = await _reservationService.GetAvailableVehiclesAsync(branchId, start, end);

            return Ok(vehicles);

        }

        catch (InvalidOperationException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize]

    [HttpPost]

    public async Task<IActionResult> Create([FromBody] CreateReservationRequestDto request)

    {

        try

        {

            var reservation = await _reservationService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = reservation.ReservationId }, reservation);

        }

        catch (InvalidOperationException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize]

    [HttpPost("{id:guid}/cancel")]

    public async Task<IActionResult> Cancel(Guid id)

    {

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value

            ?? User.FindFirst("UserId")?.Value;

        if (!int.TryParse(userIdClaim, out var userId))

            return Unauthorized();



        try

        {

            var reservation = await _reservationService.CancelAsync(id, userId);

            if (reservation == null)

                return NotFound();

            return Ok(reservation);

        }

        catch (InvalidOperationException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize(Roles = "Admin,Employee")]

    [HttpPost("{id:guid}/pickup")]

    public async Task<IActionResult> RegisterPickup(Guid id, [FromBody] RegisterTimestampRequestDto? request = null)

    {

        try

        {

            var reservation = await _reservationService.RegisterPickupAsync(id, request?.Timestamp);

            if (reservation == null)

                return NotFound();

            return Ok(reservation);

        }

        catch (InvalidOperationException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize(Roles = "Admin,Employee")]

    [HttpPost("{id:guid}/return")]

    public async Task<IActionResult> RegisterReturn(Guid id, [FromBody] RegisterTimestampRequestDto? request = null)

    {

        try

        {

            var reservation = await _reservationService.RegisterReturnAsync(id, request?.Timestamp);

            if (reservation == null)

                return NotFound();

            return Ok(reservation);

        }

        catch (InvalidOperationException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }

}

