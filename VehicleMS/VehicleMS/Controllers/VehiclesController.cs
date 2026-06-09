using Application.Dtos.Request;

using Application.Interfaces.IServices;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;



namespace VehicleMS.Controllers;



[ApiController]

[Route("api/v1/[controller]")]

public class VehiclesController : ControllerBase

{

    private readonly IVehicleService _vehicleService;



    public VehiclesController(IVehicleService vehicleService)

    {

        _vehicleService = vehicleService;

    }



    [HttpGet]

    public async Task<IActionResult> GetAll()

    {

        var vehicles = await _vehicleService.GetAllAsync();

        return Ok(vehicles);

    }



    [HttpGet("branch/{branchOfficeId:int}")]

    public async Task<IActionResult> GetByBranch(int branchOfficeId)

    {

        var vehicles = await _vehicleService.GetByBranchAsync(branchOfficeId);

        return Ok(vehicles);

    }



    [HttpGet("available")]

    public async Task<IActionResult> GetAvailable()

    {

        var vehicles = await _vehicleService.GetAvailableAsync();

        return Ok(vehicles);

    }



    [HttpGet("count-by-branch")]

    public async Task<IActionResult> GetCountByBranch()

    {

        var counts = await _vehicleService.GetCountByBranchAsync();

        return Ok(counts);

    }



    [HttpGet("{vehicleId:guid}")]

    public async Task<IActionResult> GetById(Guid vehicleId)

    {

        var vehicle = await _vehicleService.GetByIdAsync(vehicleId);

        if (vehicle == null) return NotFound();

        return Ok(vehicle);

    }



    [Authorize(Roles = "Admin")]

    [HttpPost]

    public async Task<IActionResult> Create([FromBody] CreateVehicleRequestDto request)

    {

        try

        {

            var vehicle = await _vehicleService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { vehicleId = vehicle.VehicleId }, vehicle);

        }

        catch (InvalidOperationException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize(Roles = "Admin")]

    [HttpPut("{vehicleId:guid}")]

    public async Task<IActionResult> Update(Guid vehicleId, [FromBody] UpdateVehicleRequestDto request)

    {

        try

        {

            var vehicle = await _vehicleService.UpdateAsync(vehicleId, request);

            if (vehicle == null) return NotFound();

            return Ok(vehicle);

        }

        catch (InvalidOperationException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize(Roles = "Admin")]

    [HttpDelete("{vehicleId:guid}")]

    public async Task<IActionResult> Delete(Guid vehicleId)

    {

        var deleted = await _vehicleService.DeleteAsync(vehicleId);

        if (!deleted) return NotFound();

        return NoContent();

    }



    [HttpPost("{vehicleId:guid}/rent")]

    public async Task<IActionResult> MarkAsRented(Guid vehicleId)

    {

        var updated = await _vehicleService.UpdateStatusAsync(vehicleId, 2);

        if (!updated) return NotFound();

        return Ok(new { message = "Vehículo marcado como alquilado." });

    }



    [HttpPost("{vehicleId:guid}/release")]

    public async Task<IActionResult> MarkAsAvailable(Guid vehicleId)

    {

        var updated = await _vehicleService.UpdateStatusAsync(vehicleId, 1);

        if (!updated) return NotFound();

        return Ok(new { message = "Vehículo liberado." });

    }

    [HttpPut("{vehicleId:guid}/branch")]

    public async Task<IActionResult> UpdateBranch(Guid vehicleId, [FromBody] UpdateVehicleBranchRequestDto request)

    {

        var updated = await _vehicleService.UpdateBranchAsync(vehicleId, request.BranchOfficeId);

        if (!updated) return NotFound();

        return Ok(new { message = "Sucursal del vehículo actualizada." });

    }

}

