namespace WowsKarma.Api.Data.Models.Notifications;

public record PostAddedNotification : PostNotificationBase
{
	public override NotificationType Type { get; protected private init; } = NotificationType.PostAdded;
}
