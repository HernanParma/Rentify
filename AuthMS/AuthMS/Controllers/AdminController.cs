using Application.Attributes;
using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using CustomAuthService = Application.Interfaces.IServices.IAuthorizationService;

namespace AuthMS.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequireAdmin]
    public class AdminController : ControllerBase
    {
        private readonly CustomAuthService _authorizationService;
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;
        private readonly INotificationRepository _notificationRepository;

        public AdminController(
            CustomAuthService authorizationService,
            IUserQuery userQuery,
            IUserCommand userCommand,
            INotificationRepository notificationRepository)
        {
            _authorizationService = authorizationService;
            _userQuery = userQuery;
            _userCommand = userCommand;
            _notificationRepository = notificationRepository;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = _authorizationService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });

            var result = await _userQuery.GetUserById(userId.Value);
            return Ok(result);
        }

        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            return Ok(new GenericResponse { Message = "Panel de administración Rentify activo" });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userQuery.GetAllUsersAsync();
            return Ok(users.Select(u => (UserResponse)u));
        }

        [HttpPatch("users/{id:int}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] AdminUpdateUserRoleRequest request)
        {
            if (!new[] { UserRoles.Customer, UserRoles.Admin, UserRoles.Employee }.Contains(request.Role))
                return BadRequest(new ApiError { Message = "Rol inválido." });

            var user = await _userQuery.GetUserById(id);
            if (user == null) return NotFound();

            user.Role = request.Role;
            await _userCommand.Update(user);
            return Ok((UserResponse)user);
        }

        [HttpPatch("users/{id:int}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] AdminUpdateUserStatusRequest request)
        {
            var user = await _userQuery.GetUserById(id);
            if (user == null) return NotFound();

            user.IsActive = request.IsActive;
            await _userCommand.Update(user);
            return Ok((UserResponse)user);
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int? userId,
            [FromQuery] string? status,
            [FromQuery] int limit = 100)
        {
            NotificationStatus? statusFilter = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, true, out var parsed))
                statusFilter = parsed;

            var notifications = await _notificationRepository.GetHistoryAsync(userId, statusFilter, limit);

            return Ok(notifications.Select(n => new NotificationHistoryResponse
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                UserEmail = n.User?.Email ?? "",
                Type = n.Type.ToString(),
                Status = n.Status.ToString(),
                CreatedAt = n.CreatedAt,
                SentAt = n.SentAt
            }));
        }
    }
}
