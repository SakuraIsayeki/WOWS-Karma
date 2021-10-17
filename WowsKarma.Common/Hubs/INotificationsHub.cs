using System.Threading;
using System.Threading.Tasks;
using WowsKarma.Common.Models;



namespace WowsKarma.Common.Hubs;


public interface INotificationsHubPush
{
	public Task NewNotification(INotification notification);
}

public interface INotificationsHubInvoke
{
	public Task<IAsyncEnumerable<INotification>> GetPendingNotifications(CancellationToken ct);
	public Task AcknowledgeNotifications(Guid[] notificationIds);
}
