using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;
using System.Threading;
using WowsKarma.Common.Hubs;

namespace WowsKarma.Api.Hubs;

public class NotificationsHub : Hub<INotificationsHubPush>, INotificationsHubInvoke
{
	public Task AcknowledgeNotifications(Guid[] notificationIds) => throw new NotImplementedException();

	public Task<IAsyncEnumerable<INotification>> GetPendingNotifications([EnumeratorCancellation] CancellationToken ct) => throw new NotImplementedException();
}
