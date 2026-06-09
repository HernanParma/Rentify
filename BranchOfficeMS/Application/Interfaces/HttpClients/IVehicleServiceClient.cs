namespace Application.Interfaces.HttpClients;

public interface IVehicleServiceClient
{
    Task<Dictionary<int, int>> GetAvailableCountByBranchAsync();
}
