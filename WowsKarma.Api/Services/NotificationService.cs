using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
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


	public IQueryable<NotificationBase> GetAllNotifications(uint userId) =>
		from n in _context.Set<NotificationBase>().IncludeAllNotificationsChildNavs()
		where n.AccountId == userId
		select n;

	public IQueryable<NotificationBase> GetPendingNotifications(uint userId) =>
		from n in _context.Set<NotificationBase>().IncludeAllNotificationsChildNavs()
		where n.AccountId == userId
		where n.AcknowledgedAt == null
		select n;

	public IQueryable<NotificationBase> GetNotifications(Guid[] ids) => _context.Set<NotificationBase>().Where(n => ids.Contains(n.Id));

	public async Task SendNewNotification<TNotification>(TNotification notification) where TNotification : class, INotification
	{
		_ = notification ?? throw new ArgumentNullException(nameof(notification));

		_context.Set<TNotification>().Add(notification);
		_context.SaveChanges();

		await _hub.Clients.User(notification.AccountId.ToString()).NewNotification(typeof(TNotification).FullName, notification);
		_logger.LogInformation("Sent notification {notificationId} to user {userId}.", notification.Id, notification.AccountId);
	}

	public async Task AcknowledgeNotifications(Guid[] ids)
	{
		if (ids is null or { Length: 0 })
		{
			throw new ArgumentNullException(nameof(ids));
		}

		AcknowledgeNotifications(await GetNotifications(ids).ToArrayAsync());
	}

	public void AcknowledgeNotifications(IEnumerable<NotificationBase> notifications)
	{
		if (notifications.Any())
		{
			foreach (NotificationBase notification in notifications)
			{
				notification.AcknowledgedAt = DateTime.UtcNow;
			}

			_context.SaveChanges();
			_logger.LogInformation("Acknowledged Notifications {notificationId}.", string.Join(", ", notifications.Select(n => n.Id)));
		}
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

public static class NotificationServiceExtensions
{
	public static IQueryable<NotificationBase> IncludeAllNotificationsChildNavs(this IQueryable<NotificationBase> query)
	{
		// PostAddedNotification
		query = query.Include(n => (n as PostAddedNotification).Post);

		//PostEditedNotification
		query = query.Include(n => (n as PostEditedNotification).Post);

		//PostDeletedNotification
		query = query.Include(n => (n as PostDeletedNotification).Post);

		// PostModDeletedNotification
		query = query.Include(n => (n as PostModDeletedNotification).ModAction);

		return query;
	}
}
