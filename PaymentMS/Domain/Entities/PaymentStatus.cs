namespace Domain.Entities
{
    public class PaymentStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Payment> Payments { get; set; }
    }
}
