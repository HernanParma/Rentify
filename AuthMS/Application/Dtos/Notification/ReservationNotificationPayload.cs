namespace Application.Dtos.Notification;

public class ReservationNotificationPayload
{
    public Guid ReservationId { get; set; }
    public string VehicleBrand { get; set; } = "";
    public string VehicleModel { get; set; } = "";
    public string Plate { get; set; } = "";
    public string PickupBranchName { get; set; } = "";
    public string DropOffBranchName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? ActualPickupTime { get; set; }
    public DateTime? ActualReturnTime { get; set; }
    public decimal TotalCost { get; set; }
    public string? PaymentGateway { get; set; }
    public string? TransactionId { get; set; }
}
