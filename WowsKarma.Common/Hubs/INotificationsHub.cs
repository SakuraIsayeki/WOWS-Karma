using System.Threading;
using System.Threading.Tasks;
using WowsKarma.Common.Models;



namespace WowsKarma.Common.Hubs;


public interface INotificationsHubPush
{
	public Task NewNotification(INotification notification);
	public Task DeletedNotification(Guid notificationId);
}

public interface INotificationsHubInvoke
{
	public IAsyncEnumerable<INotification> GetPendingNotifications(CancellationToken ct);
	public Task AcknowledgeNotifications(Guid[] notificationIds);
}
