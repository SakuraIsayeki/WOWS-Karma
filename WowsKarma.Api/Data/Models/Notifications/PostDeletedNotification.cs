namespace WowsKarma.Api.Data.Models.Notifications
{
	public record PostDeletedNotification : PostNotificationBase
	{
		public override NotificationType Type { get; private protected init; } = NotificationType.PostDeleted;
	}
}
