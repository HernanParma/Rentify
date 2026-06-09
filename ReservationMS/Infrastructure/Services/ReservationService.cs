using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.HttpClients;
using Application.Interfaces.IServices;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;
    private readonly IVehicleServiceClient _vehicleServiceClient;
    private readonly IBranchOfficeServiceClient _branchOfficeServiceClient;
    private readonly INotificationServiceClient _notificationClient;

    public ReservationService(
        AppDbContext context,
        IVehicleServiceClient vehicleServiceClient,
        IBranchOfficeServiceClient branchOfficeServiceClient,
        INotificationServiceClient notificationClient)
    {
        _context = context;
        _vehicleServiceClient = vehicleServiceClient;
        _branchOfficeServiceClient = branchOfficeServiceClient;
        _notificationClient = notificationClient;
    }

    public async Task<ReservationResponseDto?> GetByIdAsync(Guid id)
    {
        var reservation = await QueryReservations()
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        return reservation == null ? null : await MapToDtoAsync(reservation);
    }

    public async Task<IEnumerable<ReservationResponseDto>> GetAllAsync(ReservationFilterDto? filter = null)
    {
        var query = QueryReservations();

        if (filter != null)
        {
            if (filter.StatusId.HasValue)
                query = query.Where(r => r.ReservationStatusId == filter.StatusId.Value);

            if (filter.BranchId.HasValue)
                query = query.Where(r =>
                    r.PickUpBranchOfficeId == filter.BranchId.Value ||
                    r.DropOffBranchOfficeId == filter.BranchId.Value);

            if (filter.UserId.HasValue)
                query = query.Where(r => r.UserId == filter.UserId.Value);

            if (filter.From.HasValue)
                query = query.Where(r => r.StartTime >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(r => r.StartTime <= filter.To.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                if (int.TryParse(term, out var userIdSearch))
                    query = query.Where(r => r.UserId == userIdSearch);
                else if (Guid.TryParse(term, out var guidSearch))
                    query = query.Where(r => r.ReservationId == guidSearch);
                else
                    query = query.Where(r => r.ReservationId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase));
            }
        }

        var reservations = await query
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();

        var result = new List<ReservationResponseDto>();
        foreach (var reservation in reservations)
            result.Add(await MapToDtoAsync(reservation));

        return result;
    }

    public async Task<IEnumerable<ReservationResponseDto>> GetByUserIdAsync(int userId)
    {
        var reservations = await QueryReservations()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();

        var result = new List<ReservationResponseDto>();
        foreach (var reservation in reservations)
            result.Add(await MapToDtoAsync(reservation));

        return result;
    }

    private static readonly int[] BlockingReservationStatuses = [1, 2, 3];

    public async Task<IEnumerable<VehicleBookedRangeDto>> GetBookedRangesByVehicleAsync(Guid vehicleId)
    {
        var now = DateTime.Now;
        return await _context.Reservations.AsNoTracking()
            .Include(r => r.ReservationStatus)
            .Where(r =>
                r.VehicleId == vehicleId &&
                BlockingReservationStatuses.Contains(r.ReservationStatusId) &&
                r.EndTime > now)
            .OrderBy(r => r.StartTime)
            .Select(r => new VehicleBookedRangeDto
            {
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                ReservationStatusName = r.ReservationStatus.Name
            })
            .ToListAsync();
    }

    public async Task<ReservationResponseDto> CreateAsync(CreateReservationRequestDto request)
    {
        if (request.EndTime <= request.StartTime)
            throw new InvalidOperationException("La fecha de fin debe ser posterior a la de inicio.");

        if (request.StartTime < DateTime.Now)
            throw new InvalidOperationException("No se puede reservar con una fecha de inicio en el pasado.");

        var vehicle = await _vehicleServiceClient.GetVehicleAsync(request.VehicleId);
        if (vehicle == null)
            throw new InvalidOperationException("El vehículo no existe.");

        if (await _vehicleServiceClient.IsInMaintenanceAsync(request.VehicleId))
            throw new InvalidOperationException("El vehículo está en mantenimiento y no puede reservarse.");

        if (await HasDateConflictAsync(request.VehicleId, request.StartTime, request.EndTime))
            throw new InvalidOperationException("El vehículo ya tiene una reserva en ese período. Elegí otras fechas.");

        var fleet = await _vehicleServiceClient.GetAllVehiclesAsync();
        var fleetVehicle = fleet.FirstOrDefault(v => v.VehicleId == request.VehicleId);
        if (fleetVehicle == null)
            throw new InvalidOperationException("El vehículo no existe.");

        var timeline = await GetVehicleTimelineAsync(request.VehicleId);
        if (!VehicleAvailabilityService.IsAvailableAtBranch(
                fleetVehicle.BranchOfficeId, timeline, request.PickUpBranchOfficeId, request.StartTime, request.EndTime))
            throw new InvalidOperationException("El vehículo no estará en la sucursal de retiro en la fecha elegida.");

        var hourlyRate = await _vehicleServiceClient.GetHourlyRateAsync(request.VehicleId);
        if (hourlyRate == null)
            throw new InvalidOperationException("No se pudo obtener la tarifa del vehículo.");

        var totalHours = Math.Ceiling((request.EndTime - request.StartTime).TotalHours);
        var totalCost = (decimal)totalHours * hourlyRate.Value;

        var reservation = new Reservation
        {
            ReservationId = Guid.NewGuid(),
            UserId = request.UserId,
            VehicleId = request.VehicleId,
            ReservationStatusId = 1,
            PickUpBranchOfficeId = request.PickUpBranchOfficeId,
            DropOffBranchOfficeId = request.DropOffBranchOfficeId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            HourlyRateSnapshot = hourlyRate.Value,
            TotalCost = totalCost
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        await _context.Entry(reservation).Reference(r => r.ReservationStatus).LoadAsync();
        return await MapToDtoAsync(reservation);
    }

    public async Task<ReservationResponseDto?> ConfirmPaymentAsync(Guid reservationId, PaymentConfirmationRequestDto request)
    {
        var reservation = await _context.Reservations
            .Include(r => r.ReservationStatus)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

        if (reservation == null)
            return null;

        if (reservation.ReservationStatusId == 1)
        {
            reservation.ReservationStatusId = 2;
            reservation.TotalCost = request.TotalAmount;
            await _context.SaveChangesAsync();

            await SendNotificationAsync(reservation, "ReservationConfirmed");
            await SendNotificationAsync(reservation, "PaymentConfirmed", request.PaymentGateway, request.TransactionId);
        }

        return await MapToDtoAsync(reservation);
    }

    public async Task<ReservationResponseDto?> CancelAsync(Guid reservationId, int userId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.ReservationStatus)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

        if (reservation == null || reservation.UserId != userId)
            return null;

        if (reservation.ReservationStatusId is not (1 or 2))
            throw new InvalidOperationException("Solo se pueden cancelar reservas pendientes o confirmadas.");

        reservation.ReservationStatusId = 5;
        await _context.SaveChangesAsync();

        await SendNotificationAsync(reservation, "ReservationCancelled");

        var updated = await QueryReservations().FirstAsync(r => r.ReservationId == reservationId);
        return await MapToDtoAsync(updated);
    }

    public async Task<ReservationResponseDto?> RegisterPickupAsync(Guid reservationId, DateTime? pickupTime = null)
    {
        var reservation = await _context.Reservations
            .Include(r => r.ReservationStatus)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

        if (reservation == null)
            return null;

        if (reservation.ReservationStatusId != 2)
            throw new InvalidOperationException("Solo se puede registrar retiro en reservas confirmadas.");

        reservation.ActualPickupTime = pickupTime ?? DateTime.Now;
        reservation.ReservationStatusId = 3;
        await _context.SaveChangesAsync();
        await _context.Entry(reservation).Reference(r => r.ReservationStatus).LoadAsync();

        return await MapToDtoAsync(reservation);
    }

    public async Task<ReservationResponseDto?> RegisterReturnAsync(Guid reservationId, DateTime? returnTime = null)
    {
        var reservation = await _context.Reservations
            .Include(r => r.ReservationStatus)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

        if (reservation == null)
            return null;

        if (reservation.ReservationStatusId != 3)
            throw new InvalidOperationException("Solo se puede registrar devolución en alquileres activos.");

        reservation.ActualReturnTime = returnTime ?? DateTime.Now;
        reservation.ReservationStatusId = 4;
        await _context.SaveChangesAsync();
        await _context.Entry(reservation).Reference(r => r.ReservationStatus).LoadAsync();

        try
        {
            await _vehicleServiceClient.UpdateBranchAsync(reservation.VehicleId, reservation.DropOffBranchOfficeId);
        }
        catch { /* continuar si VehicleMS no responde */ }

        await SendNotificationAsync(reservation, "RentalCompleted");

        return await MapToDtoAsync(reservation);
    }

    public async Task<IEnumerable<AvailableVehicleDto>> GetAvailableVehiclesAsync(int branchId, DateTime start, DateTime end)
    {
        if (end <= start)
            throw new InvalidOperationException("La fecha de fin debe ser posterior a la de inicio.");

        if (start < DateTime.Now)
            throw new InvalidOperationException("No se puede buscar disponibilidad en el pasado.");

        var fleet = await _vehicleServiceClient.GetAllVehiclesAsync();
        var activeFleet = fleet.Where(v => v.VehicleStatusId != 3).ToList();

        var vehicleIds = activeFleet.Select(v => v.VehicleId).ToList();
        var allReservations = await _context.Reservations.AsNoTracking()
            .Where(r => vehicleIds.Contains(r.VehicleId) && r.ReservationStatusId != 5)
            .ToListAsync();

        var byVehicle = allReservations.GroupBy(r => r.VehicleId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<AvailableVehicleDto>();
        foreach (var vehicle in activeFleet)
        {
            var timeline = byVehicle.GetValueOrDefault(vehicle.VehicleId) ?? new List<Reservation>();
            if (!VehicleAvailabilityService.IsAvailableAtBranch(vehicle.BranchOfficeId, timeline, branchId, start, end))
                continue;

            var branchAtPickup = VehicleAvailabilityService.GetBranchAtTime(vehicle.BranchOfficeId, timeline, start);
            result.Add(new AvailableVehicleDto
            {
                VehicleId = vehicle.VehicleId,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Plate = vehicle.Plate,
                VehicleStatusId = vehicle.VehicleStatusId,
                VehicleStatusName = vehicle.VehicleStatusName,
                PricePerDay = vehicle.PricePerDay,
                BranchOfficeId = vehicle.BranchOfficeId,
                Insurance = vehicle.Insurance,
                BranchAtPickup = branchAtPickup ?? branchId
            });
        }

        return result.OrderBy(v => v.Brand).ThenBy(v => v.Model);
    }

    internal async Task SendNotificationAsync(
        Reservation reservation,
        string eventType,
        string? paymentGateway = null,
        string? transactionId = null)
    {
        var pickupName = await _branchOfficeServiceClient.GetBranchNameAsync(reservation.PickUpBranchOfficeId)
            ?? $"Sucursal {reservation.PickUpBranchOfficeId}";
        var dropoffName = await _branchOfficeServiceClient.GetBranchNameAsync(reservation.DropOffBranchOfficeId)
            ?? $"Sucursal {reservation.DropOffBranchOfficeId}";
        var vehicle = await _vehicleServiceClient.GetVehicleAsync(reservation.VehicleId);

        var payload = new
        {
            reservationId = reservation.ReservationId,
            vehicleBrand = vehicle?.Brand ?? "",
            vehicleModel = vehicle?.Model ?? "",
            plate = vehicle?.Plate ?? "",
            pickupBranchName = pickupName,
            dropOffBranchName = dropoffName,
            startTime = reservation.StartTime,
            endTime = reservation.EndTime,
            actualPickupTime = reservation.ActualPickupTime,
            actualReturnTime = reservation.ActualReturnTime,
            totalCost = reservation.TotalCost,
            paymentGateway,
            transactionId
        };

        await _notificationClient.EnqueueEventAsync(reservation.UserId, eventType, payload);
    }

    internal async Task<bool> TryRecordReminderAsync(Guid reservationId, string reminderType)
    {
        var exists = await _context.ReservationReminders
            .AnyAsync(r => r.ReservationId == reservationId && r.ReminderType == reminderType);

        if (exists) return false;

        _context.ReservationReminders.Add(new ReservationReminder
        {
            ReservationReminderId = Guid.NewGuid(),
            ReservationId = reservationId,
            ReminderType = reminderType,
            SentAt = DateTime.Now
        });
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<List<Reservation>> GetVehicleTimelineAsync(Guid vehicleId) =>
        await _context.Reservations.AsNoTracking()
            .Where(r => r.VehicleId == vehicleId && r.ReservationStatusId != 5)
            .ToListAsync();

    private async Task<bool> HasDateConflictAsync(Guid vehicleId, DateTime start, DateTime end, Guid? excludeReservationId = null)
    {
        var query = _context.Reservations.AsNoTracking()
            .Where(r =>
                r.VehicleId == vehicleId &&
                BlockingReservationStatuses.Contains(r.ReservationStatusId) &&
                r.StartTime < end &&
                r.EndTime > start);

        if (excludeReservationId.HasValue)
            query = query.Where(r => r.ReservationId != excludeReservationId.Value);

        return await query.AnyAsync();
    }

    private IQueryable<Reservation> QueryReservations() =>
        _context.Reservations.AsNoTracking().Include(r => r.ReservationStatus);

    private async Task<ReservationResponseDto> MapToDtoAsync(Reservation reservation)
    {
        var pickupName = await _branchOfficeServiceClient.GetBranchNameAsync(reservation.PickUpBranchOfficeId)
            ?? $"Sucursal {reservation.PickUpBranchOfficeId}";
        var dropoffName = await _branchOfficeServiceClient.GetBranchNameAsync(reservation.DropOffBranchOfficeId)
            ?? $"Sucursal {reservation.DropOffBranchOfficeId}";

        return new ReservationResponseDto
        {
            ReservationId = reservation.ReservationId,
            UserId = reservation.UserId,
            VehicleId = reservation.VehicleId,
            ReservationStatusId = reservation.ReservationStatusId,
            ReservationStatusName = reservation.ReservationStatus?.Name ?? "Pending",
            PickupBranchOfficeId = reservation.PickUpBranchOfficeId,
            PickupBranchOfficeName = pickupName,
            DropOffBranchOfficeId = reservation.DropOffBranchOfficeId,
            DropOffBranchOfficeName = dropoffName,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            ActualPickupTime = reservation.ActualPickupTime,
            ActualReturnTime = reservation.ActualReturnTime,
            HourlyRateSnapshot = reservation.HourlyRateSnapshot,
            TotalCost = reservation.TotalCost
        };
    }
}
