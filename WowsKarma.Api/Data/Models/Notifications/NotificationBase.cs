using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WowsKarma.Common.Models.DTOs.Notifications;

namespace WowsKarma.Api.Data.Models.Notifications;

/// <inheritdoc />
public abstract record NotificationBase : INotification
{
	/// <inheritdoc />
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	/// <inheritdoc />
	public uint AccountId { get; init; }
	
	/// <summary>
	/// The player account associated with the notification.
	/// </summary>
	public virtual Player Account { get; init; } = null!;

	/// <inheritdoc />
	public abstract NotificationType Type { get; private protected init; }

	/// <inheritdoc />
	public DateTimeOffset EmittedAt { get; private protected init; } = DateTimeOffset.UtcNow;
	
	/// <inheritdoc />
	public DateTimeOffset? AcknowledgedAt { get; set; }

	/// <summary>
	/// Converts the notification to a DTO.
	/// </summary>
	/// <returns>The DTO representation of the notification.</returns>
	public virtual NotificationBaseDTO ToDTO() => new()
	{
		Id = Id,
		AccountId = AccountId,
		AcknowledgedAt = AcknowledgedAt,
		EmittedAt = EmittedAt,
		Type = Type
	};
}
