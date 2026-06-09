namespace Application.Dtos.Response;

public class NotificationHistoryResponse
{
    public Guid NotificationId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}
