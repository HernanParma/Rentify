using System.Net.Http.Json;
using Application.Interfaces.HttpClients;

namespace Infrastructure.HttpClients;

public class BranchOfficeServiceClient : IBranchOfficeServiceClient
{
    private readonly HttpClient _httpClient;

    public BranchOfficeServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetBranchNameAsync(int branchOfficeId)
    {
        var branch = await _httpClient.GetFromJsonAsync<BranchOfficeDto>($"api/v1/BranchOffices/{branchOfficeId}");
        return branch?.Name;
    }

    private class BranchOfficeDto
    {
        public int BranchOfficeId { get; set; }
        public string Name { get; set; } = null!;
    }
}
