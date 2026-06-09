using Application.Dtos.Request;

using Application.Dtos.Response;

using Application.Interfaces.HttpClients;

using Application.Interfaces.IServices;

using Domain.Entities;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;



namespace Infrastructure.Services;



public class BranchOfficeService : IBranchOfficeService

{

    private readonly AppDbContext _context;

    private readonly IVehicleServiceClient _vehicleServiceClient;



    public BranchOfficeService(AppDbContext context, IVehicleServiceClient vehicleServiceClient)

    {

        _context = context;

        _vehicleServiceClient = vehicleServiceClient;

    }



    public async Task<IEnumerable<BranchOfficeResponseDto>> GetAllAsync()

    {

        var branches = await _context.BranchOffices.AsNoTracking().ToListAsync();

        return branches.Select(MapToDto);

    }



    public async Task<BranchOfficeResponseDto?> GetByIdAsync(int id)

    {

        var branch = await _context.BranchOffices.AsNoTracking()

            .FirstOrDefaultAsync(b => b.BranchOfficeId == id);



        return branch == null ? null : MapToDto(branch);

    }



    public async Task<IEnumerable<BranchOfficeMapResponseDto>> GetMapAsync()

    {

        var branches = await _context.BranchOffices.AsNoTracking()

            .Where(b => b.IsActive)

            .ToListAsync();



        var counts = await _vehicleServiceClient.GetAvailableCountByBranchAsync();



        return branches.Select(b => new BranchOfficeMapResponseDto

        {

            BranchOfficeId = b.BranchOfficeId,

            Name = b.Name,

            Address = b.Address,

            Phone = b.Phone,

            Hours = b.Hours,

            Latitude = b.Latitude,

            Longitude = b.Longitude,

            IsActive = b.IsActive,

            AvailableVehicleCount = counts.GetValueOrDefault(b.BranchOfficeId, 0)

        });

    }



    public async Task<BranchOfficeResponseDto> CreateAsync(CreateBranchOfficeRequestDto request)

    {

        var branch = new BranchOffice

        {

            Name = request.Name,

            Address = request.Address,

            Phone = request.Phone,

            Hours = request.Hours,

            Latitude = request.Latitude,

            Longitude = request.Longitude,

            IsActive = request.IsActive

        };



        _context.BranchOffices.Add(branch);

        await _context.SaveChangesAsync();



        return MapToDto(branch);

    }



    public async Task<BranchOfficeResponseDto?> UpdateAsync(int id, UpdateBranchOfficeRequestDto request)

    {

        var branch = await _context.BranchOffices.FirstOrDefaultAsync(b => b.BranchOfficeId == id);

        if (branch == null) return null;



        branch.Name = request.Name;

        branch.Address = request.Address;

        branch.Phone = request.Phone;

        branch.Hours = request.Hours;

        branch.Latitude = request.Latitude;

        branch.Longitude = request.Longitude;

        branch.IsActive = request.IsActive;



        await _context.SaveChangesAsync();

        return MapToDto(branch);

    }



    public async Task<bool> DeleteAsync(int id)

    {

        var branch = await _context.BranchOffices.FirstOrDefaultAsync(b => b.BranchOfficeId == id);

        if (branch == null) return false;



        _context.BranchOffices.Remove(branch);

        await _context.SaveChangesAsync();

        return true;

    }



    private static BranchOfficeResponseDto MapToDto(BranchOffice branch) => new()

    {

        BranchOfficeId = branch.BranchOfficeId,

        Name = branch.Name,

        Address = branch.Address,

        Phone = branch.Phone,

        Hours = branch.Hours,

        Latitude = branch.Latitude,

        Longitude = branch.Longitude,

        IsActive = branch.IsActive

    };

}

