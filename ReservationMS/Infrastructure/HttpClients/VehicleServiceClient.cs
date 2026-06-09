using System.Net.Http.Json;
using Application.Interfaces.HttpClients;

namespace Infrastructure.HttpClients;

public class VehicleServiceClient : IVehicleServiceClient
{
    private readonly HttpClient _httpClient;

    public VehicleServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsVehicleAvailableAsync(Guid vehicleId)
    {
        var vehicle = await GetVehicleDtoAsync(vehicleId);
        return vehicle?.VehicleStatusId == 1;
    }

    public async Task<bool> IsInMaintenanceAsync(Guid vehicleId)
    {
        var vehicle = await GetVehicleDtoAsync(vehicleId);
        return vehicle?.VehicleStatusId == 3;
    }

    public async Task<decimal?> GetHourlyRateAsync(Guid vehicleId)
    {
        var vehicle = await GetVehicleDtoAsync(vehicleId);
        return vehicle == null ? null : Math.Round(vehicle.PricePerDay / 24m, 2);
    }

    public async Task<VehicleInfo?> GetVehicleAsync(Guid vehicleId)
    {
        var vehicle = await GetVehicleDtoAsync(vehicleId);
        return vehicle == null
            ? null
            : new VehicleInfo(vehicle.VehicleId, vehicle.Brand, vehicle.Model, vehicle.Plate, vehicle.PricePerDay);
    }

    public async Task<IReadOnlyList<VehicleFleetItem>> GetAllVehiclesAsync()
    {
        var vehicles = await _httpClient.GetFromJsonAsync<List<VehicleDto>>("api/v1/Vehicles");
        if (vehicles == null) return Array.Empty<VehicleFleetItem>();

        return vehicles.Select(v => new VehicleFleetItem(
            v.VehicleId, v.Brand, v.Model, v.Year, v.Plate,
            v.VehicleStatusId, v.VehicleStatusName,
            v.PricePerDay, v.BranchOfficeId, v.Insurance)).ToList();
    }

    public async Task UpdateBranchAsync(Guid vehicleId, int branchOfficeId)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/Vehicles/{vehicleId}/branch",
            new { branchOfficeId });
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAsRentedAsync(Guid vehicleId)
    {
        var response = await _httpClient.PostAsync($"api/v1/Vehicles/{vehicleId}/rent", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAsAvailableAsync(Guid vehicleId)
    {
        var response = await _httpClient.PostAsync($"api/v1/Vehicles/{vehicleId}/release", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task<VehicleDto?> GetVehicleDtoAsync(Guid vehicleId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<VehicleDto>($"api/v1/Vehicles/{vehicleId}");
        }
        catch
        {
            return null;
        }
    }

    private class VehicleDto
    {
        public Guid VehicleId { get; set; }
        public string Brand { get; set; } = "";
        public string Model { get; set; } = "";
        public int Year { get; set; }
        public string Plate { get; set; } = "";
        public decimal PricePerDay { get; set; }
        public int VehicleStatusId { get; set; }
        public string VehicleStatusName { get; set; } = "";
        public int BranchOfficeId { get; set; }
        public string Insurance { get; set; } = "";
    }
}
