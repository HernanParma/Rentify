namespace Domain.Entities;

public class ReservationReminder
{
    public Guid ReservationReminderId { get; set; }
    public Guid ReservationId { get; set; }
    public string ReminderType { get; set; } = null!;
    public DateTime SentAt { get; set; }
}
