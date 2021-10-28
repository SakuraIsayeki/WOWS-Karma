using System.Threading;
using System.Threading.Tasks;



namespace WowsKarma.Common.Hubs;


public interface INotificationsHubPush
{
	public Task NewNotification(string dtoType, object notification);
	public Task DeletedNotification(Guid notificationId);
}

public interface INotificationsHubInvoke
{
	public IAsyncEnumerable<(string, object)> GetPendingNotifications(CancellationToken ct);
	public Task AcknowledgeNotifications(Guid[] notificationIds);
}
