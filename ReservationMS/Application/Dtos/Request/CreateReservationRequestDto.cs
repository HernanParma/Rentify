namespace Application.Dtos.Request;

public class CreateReservationRequestDto
{
    public int UserId { get; set; }
    public Guid VehicleId { get; set; }
    public int PickUpBranchOfficeId { get; set; }
    public int DropOffBranchOfficeId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
