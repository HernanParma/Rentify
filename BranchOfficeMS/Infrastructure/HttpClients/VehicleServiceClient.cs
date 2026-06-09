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

    public async Task<Dictionary<int, int>> GetAvailableCountByBranchAsync()
    {
        var counts = await _httpClient.GetFromJsonAsync<List<BranchCountDto>>("api/v1/Vehicles/count-by-branch");
        return counts?.ToDictionary(c => c.BranchOfficeId, c => c.AvailableCount)
            ?? new Dictionary<int, int>();
    }

    private class BranchCountDto
    {
        public int BranchOfficeId { get; set; }
        public int AvailableCount { get; set; }
    }
}
