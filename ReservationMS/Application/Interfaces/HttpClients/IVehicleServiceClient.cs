namespace Application.Interfaces.HttpClients;



public record VehicleInfo(Guid VehicleId, string Brand, string Model, string Plate, decimal PricePerDay);

public record VehicleFleetItem(
    Guid VehicleId,
    string Brand,
    string Model,
    int Year,
    string Plate,
    int VehicleStatusId,
    string VehicleStatusName,
    decimal PricePerDay,
    int BranchOfficeId,
    string Insurance);

public interface IVehicleServiceClient
{
    Task<bool> IsVehicleAvailableAsync(Guid vehicleId);
    Task<bool> IsInMaintenanceAsync(Guid vehicleId);
    Task<decimal?> GetHourlyRateAsync(Guid vehicleId);
    Task<VehicleInfo?> GetVehicleAsync(Guid vehicleId);
    Task<IReadOnlyList<VehicleFleetItem>> GetAllVehiclesAsync();
    Task UpdateBranchAsync(Guid vehicleId, int branchOfficeId);
    Task MarkAsRentedAsync(Guid vehicleId);
    Task MarkAsAvailableAsync(Guid vehicleId);
}

