using Domain.Entities;

namespace Infrastructure.Services;

public static class VehicleAvailabilityService
{
    private static readonly int[] OverlapStatuses = [1, 2, 3];

    /// <summary>
    /// Sucursal donde está el vehículo al momento del retiro, o null si está alquilado.
    /// </summary>
    public static int? GetBranchAtTime(int currentBranchInDb, IEnumerable<Reservation> reservations, DateTime moment)
    {
        var timeline = reservations
            .Where(r => r.ReservationStatusId != 5)
            .OrderBy(r => r.StartTime)
            .ToList();

        var location = currentBranchInDb;

        foreach (var res in timeline)
        {
            if (moment < res.StartTime)
                break;

            if (moment < res.EndTime && OverlapStatuses.Contains(res.ReservationStatusId))
                return null;

            if (moment >= res.EndTime && res.ReservationStatusId is 2 or 3 or 4)
                location = res.DropOffBranchOfficeId;
        }

        return location;
    }

    public static bool HasDateOverlap(IEnumerable<Reservation> reservations, DateTime start, DateTime end, Guid? excludeReservationId = null)
    {
        return reservations
            .Where(r => OverlapStatuses.Contains(r.ReservationStatusId))
            .Where(r => !excludeReservationId.HasValue || r.ReservationId != excludeReservationId)
            .Any(r => r.StartTime < end && r.EndTime > start);
    }

    public static bool IsAvailableAtBranch(
        int currentBranchInDb,
        IEnumerable<Reservation> reservations,
        int branchId,
        DateTime start,
        DateTime end)
    {
        if (HasDateOverlap(reservations, start, end))
            return false;

        var branchAtPickup = GetBranchAtTime(currentBranchInDb, reservations, start);
        return branchAtPickup == branchId;
    }
}
