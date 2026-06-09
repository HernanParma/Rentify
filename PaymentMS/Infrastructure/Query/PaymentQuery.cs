using Domain.Entities;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Query
{
    public class PaymentQuery : IPaymentQuery
    {
        private readonly AppDbContext _context;

        public PaymentQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        public async Task<List<Payment>> GetPaymentsByDateAsync(DateTime date)
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .Where(p => p.Date.Date == date.Date)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByReservationIdAsync(Guid reservationId)
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);
        }

        public async Task<List<Payment>> GetPaymentsByStatusIdAsync(int status)
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .Where(p => p.PaymentStatusId == status)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetPaymentsByMethodIdAsync(int paymentMethodId)
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .Where(p => p.PaymentMethodId == paymentMethodId)
                .ToListAsync();
        }
    }
}
