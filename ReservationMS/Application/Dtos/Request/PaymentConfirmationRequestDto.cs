namespace Application.Dtos.Request;

public class PaymentConfirmationRequestDto
{
    public decimal TotalAmount { get; set; }
    public decimal LateFee { get; set; }
    public string PaymentGateway { get; set; } = null!;
    public string TransactionId { get; set; } = null!;
}
