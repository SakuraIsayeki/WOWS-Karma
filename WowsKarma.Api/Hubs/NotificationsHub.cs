using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Services;
using WowsKarma.Common.Hubs;

namespace WowsKarma.Api.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub<INotificationsHubPush>, INotificationsHubInvoke
{
	private readonly NotificationService _service;

	public NotificationsHub(NotificationService service)
	{
		_service = service;
	}

	public Task AcknowledgeNotifications(Guid[] notificationIds)
	{
		uint userId = uint.Parse(Context.UserIdentifier ?? throw new InvalidOperationException("No context user identifier. Is the user logged in on the hub?"));

		IQueryable<NotificationBase> notifications = _service.GetNotifications(notificationIds).Where(n => n.AccountId == userId); 
		
		_service.AcknowledgeNotifications(notifications);
		return Task.CompletedTask;
	}

	public async IAsyncEnumerable<(string, object)> GetPendingNotifications([EnumeratorCancellation] CancellationToken ct)
	{
		uint userId = uint.Parse(Context.UserIdentifier ?? throw new InvalidOperationException("No context user identifier. Is the user logged in on the hub?"));
		
		ConfiguredCancelableAsyncEnumerable<NotificationBase> notifications = _service.GetPendingNotifications(userId)
			.AsNoTracking()
			.AsAsyncEnumerable()
			.WithCancellation(ct);

		await foreach (NotificationBase item in notifications)
		{
			ct.ThrowIfCancellationRequested();
			object notificationDto = item.ToDTO();

			yield return (notificationDto.GetType().FullName!, notificationDto);
		}
	}
}
