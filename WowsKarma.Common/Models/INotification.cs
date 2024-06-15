using JetBrains.Annotations;

namespace WowsKarma.Common.Models;

/// <summary>
/// Represents a notification.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface INotification
{
	/// <summary>
	/// The unique identifier of the notification.
	/// </summary>
	public Guid Id { get; }

	/// <summary>
	/// The unique identifier of the player who the notification is for.
	/// </summary>
	public uint AccountId { get; }

	/// <summary>
	/// The player who the notification is for.
	/// </summary>
	public NotificationType Type { get; }

	/// <summary>
	/// The date and time the notification was emitted.
	/// </summary>
	public DateTimeOffset EmittedAt { get; }

	/// <summary>
	/// The date and time the notification was acknowledged.
	/// </summary>
	public DateTimeOffset? AcknowledgedAt { get; }
}