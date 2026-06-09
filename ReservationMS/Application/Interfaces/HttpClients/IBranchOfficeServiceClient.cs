namespace Application.Interfaces.HttpClients;

public interface IBranchOfficeServiceClient
{
    Task<string?> GetBranchNameAsync(int branchOfficeId);
}
