namespace Application.Dtos.Response
{
    public class PaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public Guid ReservationId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethodName { get; set; }
        public string PaymentStatusName { get; set; }
    }
}
