using System.ComponentModel.DataAnnotations;
using WowsKarma.Common.Models.DTOs.Notifications;


namespace WowsKarma.Api.Data.Models.Notifications;


public record PlatformBanNotification : NotificationBase
{
	public override NotificationType Type { get; private protected init; } = NotificationType.PlatformBan;

	[Required]
	public Guid BanId { get; set; }
	public PlatformBan Ban { get; set; }


	public override PlatformBanNotificationDTO ToDTO() => new()
	{
		Id = Id,
		AccountId = AccountId,
		AcknowledgedAt = AcknowledgedAt,
		EmittedAt = EmittedAt,
		Type = Type,
		Reason = Ban.Reason,
		Until = Ban.BannedUntil
	};
}
