namespace Domain.Entities;

public class Reservation
{
    public Guid ReservationId { get; set; }
    public int UserId { get; set; }
    public Guid VehicleId { get; set; }
    public int ReservationStatusId { get; set; }
    public int PickUpBranchOfficeId { get; set; }
    public int DropOffBranchOfficeId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? ActualPickupTime { get; set; }
    public DateTime? ActualReturnTime { get; set; }
    public decimal HourlyRateSnapshot { get; set; }
    public decimal TotalCost { get; set; }
    public ReservationStatus ReservationStatus { get; set; } = null!;
}
