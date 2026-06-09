namespace Domain.Entities;

public class ReservationStatus
{
    public int ReservationStatusId { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
