using Application.Dtos.Request;
using Application.Dtos.Response;

namespace Application.Interfaces.IServices;

public interface IVehicleService
{
    Task<IEnumerable<VehicleResponseDto>> GetAllAsync();
    Task<IEnumerable<VehicleResponseDto>> GetByBranchAsync(int branchOfficeId);
    Task<IEnumerable<VehicleResponseDto>> GetAvailableAsync();
    Task<IEnumerable<BranchVehicleCountDto>> GetCountByBranchAsync();
    Task<VehicleResponseDto?> GetByIdAsync(Guid vehicleId);
    Task<bool> IsAvailableAsync(Guid vehicleId);
    Task<decimal?> GetHourlyRateAsync(Guid vehicleId);
    Task<bool> UpdateStatusAsync(Guid vehicleId, int vehicleStatusId);
    Task<bool> UpdateBranchAsync(Guid vehicleId, int branchOfficeId);
    Task<VehicleResponseDto> CreateAsync(CreateVehicleRequestDto request);
    Task<VehicleResponseDto?> UpdateAsync(Guid vehicleId, UpdateVehicleRequestDto request);
    Task<bool> DeleteAsync(Guid vehicleId);
}
