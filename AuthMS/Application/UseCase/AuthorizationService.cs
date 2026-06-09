using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.UseCase
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool HasRole(string role)
        {
            var user = GetCurrentUser();
            return user != null && user.IsInRole(role);
        }

        public bool HasAnyRole(params string[] roles)
        {
            var user = GetCurrentUser();
            return user != null && roles.Any(role => user.IsInRole(role));
        }

        public int? GetCurrentUserId()
        {
            var user = GetCurrentUser();
            if (user == null) return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            return null;
        }

        public string? GetCurrentUserRole()
        {
            return GetCurrentUser()?.FindFirst(ClaimTypes.Role)?.Value;
        }

        public bool CanAccessUserData(int targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return false;
            if (IsAdmin()) return true;
            return currentUserId == targetUserId;
        }

        public bool IsAdmin()
        {
            return HasRole(UserRoles.Admin);
        }

        public bool IsCustomer()
        {
            return HasRole(UserRoles.Customer);
        }

        public ClaimsPrincipal? GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User;
        }
    }
}
