using Application.Attributes;
using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IServices;
using Application.Interfaces.IQuery;
using Microsoft.AspNetCore.Mvc;
using CustomAuthService = Application.Interfaces.IServices.IAuthorizationService;

namespace AuthMS.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequireCustomer]
    public class CustomerController : ControllerBase
    {
        private readonly CustomAuthService _authorizationService;
        private readonly IUserQuery _userQuery;

        public CustomerController(CustomAuthService authorizationService, IUserQuery userQuery)
        {
            _authorizationService = authorizationService;
            _userQuery = userQuery;
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

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile(UserUpdateRequest request)
        {
            var userId = _authorizationService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });

            var result = await _userQuery.GetUserById(userId.Value);
            return Ok(result);
        }

        [HttpGet("reservations")]
        public IActionResult GetMyReservations()
        {
            var userId = _authorizationService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ApiError { Message = "Usuario no autenticado" });

            return Ok(new GenericResponse { Message = $"Consultar reservas en ReservationMS para el usuario {userId}" });
        }
    }
}
