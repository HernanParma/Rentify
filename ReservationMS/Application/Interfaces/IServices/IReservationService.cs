using Application.Dtos.Request;

using Application.Dtos.Response;



namespace Application.Interfaces.IServices;



public interface IReservationService

{

    Task<ReservationResponseDto?> GetByIdAsync(Guid id);

    Task<IEnumerable<ReservationResponseDto>> GetAllAsync(ReservationFilterDto? filter = null);

    Task<IEnumerable<ReservationResponseDto>> GetByUserIdAsync(int userId);

    Task<ReservationResponseDto> CreateAsync(CreateReservationRequestDto request);

    Task<ReservationResponseDto?> ConfirmPaymentAsync(Guid reservationId, PaymentConfirmationRequestDto request);

    Task<ReservationResponseDto?> CancelAsync(Guid reservationId, int userId);

    Task<ReservationResponseDto?> RegisterPickupAsync(Guid reservationId, DateTime? pickupTime = null);

    Task<ReservationResponseDto?> RegisterReturnAsync(Guid reservationId, DateTime? returnTime = null);

    Task<IEnumerable<VehicleBookedRangeDto>> GetBookedRangesByVehicleAsync(Guid vehicleId);

    Task<IEnumerable<AvailableVehicleDto>> GetAvailableVehiclesAsync(int branchId, DateTime start, DateTime end);

}

