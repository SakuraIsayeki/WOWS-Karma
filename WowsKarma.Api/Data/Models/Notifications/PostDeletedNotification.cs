namespace WowsKarma.Api.Data.Models.Notifications
{
	public record PostDeletedNotification : PostNotificationBase
	{
		public override NotificationType Type { get; protected private init; } = NotificationType.PostDeleted;
	}
}
