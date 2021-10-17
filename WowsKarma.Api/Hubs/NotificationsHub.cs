using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Threading;
using WowsKarma.Api.Services;
using WowsKarma.Common.Hubs;



namespace WowsKarma.Api.Hubs;


[Authorize]
public class NotificationsHub : Hub<INotificationsHubPush>, INotificationsHubInvoke
{
	private readonly NotificationService _service;

	public NotificationsHub(NotificationService service)
	{
		_service = service;
	}

	public Task AcknowledgeNotifications(Guid[] notificationIds) => _service.AcknowledgeNotifications(notificationIds);

	public async IAsyncEnumerable<INotification> GetPendingNotifications([EnumeratorCancellation] CancellationToken ct)
	{
		ConfiguredCancelableAsyncEnumerable<INotification> notifications = _service.GetPendingNotifications(uint.Parse(Context.UserIdentifier))
			.AsAsyncEnumerable().WithCancellation(ct);

		await foreach (INotification item in notifications)
		{
			ct.ThrowIfCancellationRequested();
			yield return item;
		}
	}
}
