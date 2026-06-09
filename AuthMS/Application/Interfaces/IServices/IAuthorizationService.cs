using System.Security.Claims;

namespace Application.Interfaces.IServices
{
    public interface IAuthorizationService
    {
        bool HasRole(string role);
        bool HasAnyRole(params string[] roles);
        int? GetCurrentUserId();
        string? GetCurrentUserRole();
        bool CanAccessUserData(int targetUserId);
        bool IsAdmin();
        bool IsCustomer();
        ClaimsPrincipal? GetCurrentUser();
    }
}
