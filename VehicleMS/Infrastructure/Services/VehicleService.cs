using Application.Dtos.Request;

using Application.Dtos.Response;

using Application.Interfaces.IServices;

using Domain.Entities;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;



namespace Infrastructure.Services;



public class VehicleService : IVehicleService

{

    private readonly AppDbContext _context;



    public VehicleService(AppDbContext context)

    {

        _context = context;

    }



    public async Task<IEnumerable<VehicleResponseDto>> GetAllAsync()

    {

        var vehicles = await QueryVehicles().ToListAsync();

        return vehicles.Select(MapToDto);

    }



    public async Task<IEnumerable<VehicleResponseDto>> GetByBranchAsync(int branchOfficeId)

    {

        var vehicles = await QueryVehicles()

            .Where(v => v.BranchOfficeId == branchOfficeId)

            .ToListAsync();



        return vehicles.Select(MapToDto);

    }



    public async Task<IEnumerable<VehicleResponseDto>> GetAvailableAsync()

    {

        var vehicles = await QueryVehicles()

            .Where(v => v.VehicleStatusId == 1)

            .ToListAsync();



        return vehicles.Select(MapToDto);

    }



    public async Task<IEnumerable<BranchVehicleCountDto>> GetCountByBranchAsync()

    {

        return await _context.Vehicles.AsNoTracking()

            .Where(v => v.VehicleStatusId != 3)

            .GroupBy(v => v.BranchOfficeId)

            .Select(g => new BranchVehicleCountDto

            {

                BranchOfficeId = g.Key,

                AvailableCount = g.Count()

            })

            .ToListAsync();

    }



    public async Task<VehicleResponseDto?> GetByIdAsync(Guid vehicleId)

    {

        var vehicle = await QueryVehicles()

            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);



        return vehicle == null ? null : MapToDto(vehicle);

    }



    public async Task<bool> IsAvailableAsync(Guid vehicleId)

    {

        return await _context.Vehicles.AsNoTracking()

            .AnyAsync(v => v.VehicleId == vehicleId && v.VehicleStatusId == 1);

    }



    public async Task<decimal?> GetHourlyRateAsync(Guid vehicleId)

    {

        var vehicle = await _context.Vehicles.AsNoTracking()

            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);



        return vehicle == null ? null : Math.Round(vehicle.PricePerDay / 24m, 2);

    }



    public async Task<bool> UpdateStatusAsync(Guid vehicleId, int vehicleStatusId)

    {

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

        if (vehicle == null) return false;



        vehicle.VehicleStatusId = vehicleStatusId;

        await _context.SaveChangesAsync();

        return true;

    }

    public async Task<bool> UpdateBranchAsync(Guid vehicleId, int branchOfficeId)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);
        if (vehicle == null) return false;

        vehicle.BranchOfficeId = branchOfficeId;
        if (vehicle.VehicleStatusId == 2)
            vehicle.VehicleStatusId = 1;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<VehicleResponseDto> CreateAsync(CreateVehicleRequestDto request)

    {

        if (await _context.Vehicles.AnyAsync(v => v.Plate == request.Plate))

            throw new InvalidOperationException("Ya existe un vehículo con esa patente.");



        var vehicle = new Vehicle

        {

            VehicleId = Guid.NewGuid(),

            Brand = request.Brand,

            Model = request.Model,

            Year = request.Year,

            Plate = request.Plate,

            VehicleStatusId = request.VehicleStatusId,

            PricePerDay = request.PricePerDay,

            BranchOfficeId = request.BranchOfficeId,

            Insurance = request.Insurance

        };



        _context.Vehicles.Add(vehicle);

        await _context.SaveChangesAsync();



        return (await GetByIdAsync(vehicle.VehicleId))!;

    }



    public async Task<VehicleResponseDto?> UpdateAsync(Guid vehicleId, UpdateVehicleRequestDto request)

    {

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

        if (vehicle == null) return null;



        if (await _context.Vehicles.AnyAsync(v => v.Plate == request.Plate && v.VehicleId != vehicleId))

            throw new InvalidOperationException("Ya existe otro vehículo con esa patente.");



        vehicle.Brand = request.Brand;

        vehicle.Model = request.Model;

        vehicle.Year = request.Year;

        vehicle.Plate = request.Plate;

        vehicle.VehicleStatusId = request.VehicleStatusId;

        vehicle.PricePerDay = request.PricePerDay;

        vehicle.BranchOfficeId = request.BranchOfficeId;

        vehicle.Insurance = request.Insurance;



        await _context.SaveChangesAsync();

        return await GetByIdAsync(vehicleId);

    }



    public async Task<bool> DeleteAsync(Guid vehicleId)

    {

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

        if (vehicle == null) return false;



        _context.Vehicles.Remove(vehicle);

        await _context.SaveChangesAsync();

        return true;

    }



    private IQueryable<Vehicle> QueryVehicles() =>

        _context.Vehicles.AsNoTracking().Include(v => v.VehicleStatus);



    private static VehicleResponseDto MapToDto(Vehicle vehicle) => new()

    {

        VehicleId = vehicle.VehicleId,

        Brand = vehicle.Brand,

        Model = vehicle.Model,

        Year = vehicle.Year,

        Plate = vehicle.Plate,

        VehicleStatusId = vehicle.VehicleStatusId,

        VehicleStatusName = vehicle.VehicleStatus.Name,

        PricePerDay = vehicle.PricePerDay,

        BranchOfficeId = vehicle.BranchOfficeId,

        Insurance = vehicle.Insurance

    };

}

