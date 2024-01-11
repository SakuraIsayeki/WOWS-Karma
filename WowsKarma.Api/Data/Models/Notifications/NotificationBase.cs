using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WowsKarma.Common.Models.DTOs.Notifications;

namespace WowsKarma.Api.Data.Models.Notifications;

public abstract record NotificationBase : INotification
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	public uint AccountId { get; init; }
	public virtual Player Account { get; init; }

	public abstract NotificationType Type { get; private protected init; }

	public DateTimeOffset EmittedAt { get; private protected init; } = DateTimeOffset.UtcNow;
	public DateTimeOffset? AcknowledgedAt { get; set; }

	public virtual NotificationBaseDTO ToDTO() => new()
	{
		Id = Id,
		AccountId = AccountId,
		AcknowledgedAt = AcknowledgedAt,
		EmittedAt = EmittedAt,
		Type = Type
	};
}
