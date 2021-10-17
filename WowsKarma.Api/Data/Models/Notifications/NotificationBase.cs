namespace WowsKarma.Api.Data.Models.Notifications;

public abstract record NotificationBase : INotification
{
	public Guid Id { get; init; }

	public uint AccountId { get; init; }
	public virtual Player Account { get; init; }

	public abstract NotificationType Type { get; protected private init; }

	public DateTime EmittedAt { get; init; }
	public DateTime? AcknowledgedAt { get; set; }
}
