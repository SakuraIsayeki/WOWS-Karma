namespace WowsKarma.Api.Data.Models.Notifications;

public record PostAddedNotification : PostNotificationBase
{
	public override NotificationType Type { get; private protected init; } = NotificationType.PostAdded;
}
