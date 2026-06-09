namespace Application.Dtos.Response;

public class VehicleBookedRangeDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ReservationStatusName { get; set; } = null!;
}
