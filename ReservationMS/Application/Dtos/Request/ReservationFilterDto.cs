namespace Application.Dtos.Request;

public class ReservationFilterDto
{
    public int? StatusId { get; set; }
    public int? BranchId { get; set; }
    public int? UserId { get; set; }
    public string? Search { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
