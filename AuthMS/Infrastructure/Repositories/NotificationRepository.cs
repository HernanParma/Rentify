using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context; 
        public NotificationRepository(AppDbContext context) => _context = context;

        public async Task Add(Notification entity)
        {
            _context.Notifications.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetPending()
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Pending)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetHistoryAsync(int? userId = null, NotificationStatus? status = null, int limit = 100)
        {
            var query = _context.Notifications
                .Include(n => n.User)
                .AsNoTracking()
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(n => n.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(n => n.Status == status.Value);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task Update(Notification entity)
        {
            _context.Notifications.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
