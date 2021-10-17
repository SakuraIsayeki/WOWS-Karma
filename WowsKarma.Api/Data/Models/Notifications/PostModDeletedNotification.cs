namespace WowsKarma.Api.Data.Models.Notifications;

public record PostModDeletedNotification : NotificationBase
{
	public override NotificationType Type { get; protected private init; } = NotificationType.PostModDeleted;

	public virtual PostModAction ModAction { get; set; }

}
