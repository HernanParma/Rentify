namespace Domain.Entities
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Payment> Payments { get; set; }
    }
}
