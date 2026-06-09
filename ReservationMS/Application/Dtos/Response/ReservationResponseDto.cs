namespace Application.Dtos.Response;

public class ReservationResponseDto
{
    public Guid ReservationId { get; set; }
    public int UserId { get; set; }
    public Guid VehicleId { get; set; }
    public int ReservationStatusId { get; set; }
    public string ReservationStatusName { get; set; } = null!;
    public int PickupBranchOfficeId { get; set; }
    public string PickupBranchOfficeName { get; set; } = null!;
    public int DropOffBranchOfficeId { get; set; }
    public string DropOffBranchOfficeName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? ActualPickupTime { get; set; }
    public DateTime? ActualReturnTime { get; set; }
    public decimal HourlyRateSnapshot { get; set; }
    public decimal TotalCost { get; set; }
}
