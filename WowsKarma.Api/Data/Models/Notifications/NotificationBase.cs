using WowsKarma.Common.Models.DTOs.Notifications;

namespace WowsKarma.Api.Data.Models.Notifications;

public abstract record NotificationBase : INotification
{
	public Guid Id { get; init; }

	public uint AccountId { get; init; }
	public virtual Player Account { get; init; }

	public abstract NotificationType Type { get; protected private init; }

	public DateTime EmittedAt { get; protected private init; } = DateTime.UtcNow;
	public DateTime? AcknowledgedAt { get; set; }

	public virtual NotificationDTO ToDTO() => new()
	{
		Id = Id,
		AccountId = AccountId,
		AcknowledgedAt = AcknowledgedAt,
		EmittedAt = EmittedAt,
		Type = Type
	};
}
