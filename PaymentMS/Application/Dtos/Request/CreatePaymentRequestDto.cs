namespace Application.Dtos.Request
{
    public class CreatePaymentRequestDto
    {
        public Guid ReservationId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
    }
}
