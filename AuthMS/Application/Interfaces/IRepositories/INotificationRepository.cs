using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IRepositories
{
    public interface INotificationRepository
    {
        Task Add(Notification notification);
        Task Update(Notification notification);
        Task<List<Notification>> GetPending();
        Task<List<Notification>> GetHistoryAsync(int? userId = null, NotificationStatus? status = null, int limit = 100);
    }
}
