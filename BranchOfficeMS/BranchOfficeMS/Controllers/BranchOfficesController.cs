using Application.Dtos.Request;

using Application.Interfaces.IServices;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;



namespace BranchOfficeMS.Controllers;



[ApiController]

[Route("api/v1/[controller]")]

public class BranchOfficesController : ControllerBase

{

    private readonly IBranchOfficeService _branchOfficeService;



    public BranchOfficesController(IBranchOfficeService branchOfficeService)

    {

        _branchOfficeService = branchOfficeService;

    }



    [HttpGet]

    public async Task<IActionResult> GetAll()

    {

        var branches = await _branchOfficeService.GetAllAsync();

        return Ok(branches);

    }



    [HttpGet("map")]

    public async Task<IActionResult> GetMap()

    {

        var branches = await _branchOfficeService.GetMapAsync();

        return Ok(branches);

    }



    [HttpGet("{id:int}")]

    public async Task<IActionResult> GetById(int id)

    {

        var branch = await _branchOfficeService.GetByIdAsync(id);

        if (branch == null)

            return NotFound();



        return Ok(branch);

    }



    [Authorize(Roles = "Admin")]

    [HttpPost]

    public async Task<IActionResult> Create([FromBody] CreateBranchOfficeRequestDto request)

    {

        var branch = await _branchOfficeService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = branch.BranchOfficeId }, branch);

    }



    [Authorize(Roles = "Admin")]

    [HttpPut("{id:int}")]

    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchOfficeRequestDto request)

    {

        var branch = await _branchOfficeService.UpdateAsync(id, request);

        if (branch == null) return NotFound();

        return Ok(branch);

    }



    [Authorize(Roles = "Admin")]

    [HttpDelete("{id:int}")]

    public async Task<IActionResult> Delete(int id)

    {

        var deleted = await _branchOfficeService.DeleteAsync(id);

        if (!deleted) return NotFound();

        return NoContent();

    }

}

