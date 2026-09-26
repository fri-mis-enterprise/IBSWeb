using IBS.Models;

namespace IBS.DataAccess.Repository.IRepository
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(string userId, string message, bool requiresResponse = false);

        Task AddNotificationToMultipleUsersAsync(List<string> userIds, string message, bool requiresResponse = false);

        Task<List<UserNotification>> GetUserNotificationsAsync(string userId);

        Task<bool> MarkAsReadAsync(Guid userNotificationId, string userId);

        Task<bool> MarkAsUnreadAsync(Guid userNotificationId, string userId);

        Task<int> GetUnreadNotificationCountAsync(string userId);

        Task ArchiveAsync(Guid userNotificationId);

        Task MarkAllAsReadAsync(string userId, CancellationToken cancellation = default);

        Task ArchiveAllAsync(string userId, CancellationToken cancellation = default);
    }
}
