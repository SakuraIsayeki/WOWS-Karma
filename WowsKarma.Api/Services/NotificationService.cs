using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Hubs;
using WowsKarma.Common.Hubs;
using WowsKarma.Common.Models.DTOs.Notifications;

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

	public TNotification? GetNotification<TNotification>(Guid id) where TNotification : NotificationBase 
		=> _context.Set<TNotification>().FirstOrDefault(n => n.Id == id);
	
	public async Task SendNewNotification<TNotification>(TNotification notification) where TNotification : NotificationBase
	{
		_ = notification ?? throw new ArgumentNullException(nameof(notification));

		EntityEntry<TNotification> entry = _context.Set<TNotification>().Add(notification);
		await _context.SaveChangesAsync();
		
		// Populate a DTO with the notification data
		NotificationBaseDTO dto = entry.Entity.ToDTO();

		await _hub.Clients.User(notification.AccountId.ToString()).NewNotification(typeof(TNotification).FullName!, dto);
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
			List<Guid> ids = [];
			
			foreach (NotificationBase notification in notifications)
			{
				notification.AcknowledgedAt = DateTime.UtcNow;
				ids.Add(notification.Id);
			}

			_context.SaveChanges();
			_logger.LogInformation("Acknowledged Notifications {notificationId}.", string.Join(", ", ids));
		}
	}

	public async Task DeleteNotificationAsync(Guid id)
	{
		NotificationBase notification = await _context.Set<NotificationBase>().FindAsync(id) ?? throw new ArgumentException("No notification found for given ID.", nameof(id));
		_context.Remove(notification);
		await _context.SaveChangesAsync();
		_logger.LogInformation("Removed Notification {id}.", id);
		await _hub.Clients.All.DeletedNotification(id);
	}
}

public static class NotificationServiceExtensions
{
	public static IQueryable<NotificationBase> IncludeAllNotificationsChildNavs(this IQueryable<NotificationBase> query)
	{
		//PlatformBanNotification
		query = query.Include(static n => (n as PlatformBanNotification)!.Ban);
		

		// PostAddedNotification
		query = query.IncludeAllPostNotificationsChildNavs<PostAddedNotification>();

		//PostEditedNotification
		query = query.IncludeAllPostNotificationsChildNavs<PostEditedNotification>();

		// PostModEditedNotification
		query = query.Include(static n => (n as PostModEditedNotification)!.ModAction);

		// PostModDeletedNotification
		query = query.Include(static n => (n as PostModDeletedNotification)!.ModAction);

		return query;
	}
	
	/// <summary>
	/// Returns a queryable of type <typeparamref name="TNotification"/> that includes all <see cref="PostNotificationBase"/> child navigation properties.
	/// </summary>
	/// <returns>An included queryable of type <typeparamref name="TNotification"/> that includes all <see cref="PostNotificationBase"/> child navigation properties. </returns>
	[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IQueryable<NotificationBase> IncludeAllPostNotificationsChildNavs<TNotification>(this IQueryable<NotificationBase> query) 
		where TNotification : PostNotificationBase
	{
		query = query.Include(static n => (n as TNotification)!.Post)
			.ThenInclude(static p => p.Author);

		query = query.Include(static n => (n as TNotification)!.Post)
			.ThenInclude(static p => p.Player);
		
		return query;
	}
}
