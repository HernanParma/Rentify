using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPaymentQuery
    {
        Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
        Task<List<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByReservationIdAsync(Guid reservationId);
        Task<List<Payment>> GetPaymentsByDateAsync(DateTime date);
        Task<List<Payment>> GetPaymentsByStatusIdAsync(int status);
        Task<List<Payment>> GetPaymentsByMethodIdAsync(int methodId);
    }
}
