namespace WowsKarma.Api.Data.Models.Notifications;

public record PostEditedNotification : PostNotificationBase
{
	public override NotificationType Type { get; protected private init; } = NotificationType.PostEdited;
}
