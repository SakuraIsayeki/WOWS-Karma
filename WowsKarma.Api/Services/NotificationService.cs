using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Hubs;
using WowsKarma.Common.Hubs;



namespace WowsKarma.Api.Services;


public class NotificationService
{
	private readonly ILogger<NotificationService> _logger;
	private readonly ApiDbContext _context;
	private readonly IHubContext<NotificationsHub, INotificationsHubPush> _hub;

	public NotificationService(ILogger<NotificationService> logger, ApiDbContext dbContext, IHubContext<NotificationsHub, INotificationsHubPush> hub)
	{
		_logger = logger;
		_context = dbContext;
		_hub = hub;
	}


	public IQueryable<INotification> GetAllNotifications(uint userId) =>
		from n in _context.Notifications
		where n.AccountId == userId
		select n;

	public IQueryable<INotification> GetPendingNotifications(uint userId) =>
		from n in _context.Notifications
		where n.AccountId == userId
		where n.AcknowledgedAt == null
		select n;

	public async Task SendNewNotification<TNotification>(TNotification notification) where TNotification : class, INotification
	{
		_ = notification ?? throw new ArgumentNullException(nameof(notification));
		_context.Set<TNotification>().Add(notification);
		_context.SaveChanges();

		await _hub.Clients.User(notification.AccountId.ToString()).NewNotification(notification);
		_logger.LogInformation("Sent notification {notificationId} to user {userId}.", notification.Id, notification.AccountId);
	}

	public async Task AcknowledgeNotifications(Guid[] ids)
	{
		if (ids is null or { Length: 0 })
		{
			throw new ArgumentNullException(nameof(ids));
		}

		NotificationBase[] notifications = await _context.Set<NotificationBase>().Where(n => ids.Contains(n.Id)).ToArrayAsync();

		foreach (NotificationBase notification in notifications)
		{
			notification.AcknowledgedAt = DateTime.UtcNow;
		}

		_context.SaveChanges();
		_logger.LogInformation("Acknowledged Notifications {notificationId}.", string.Join(", ", notifications.Select(n => n.Id)));
	}

	public async Task DeleteNotification(Guid id)
	{
		NotificationBase notification = await _context.Set<NotificationBase>().FindAsync(id) ?? throw new ArgumentException("No notification found for given ID.", nameof(id));
		_context.Remove(notification);
		_context.SaveChanges();

		_logger.LogInformation("Removed Notification {id}.", id);

		await _hub.Clients.All.DeletedNotification(id);
	}
}
