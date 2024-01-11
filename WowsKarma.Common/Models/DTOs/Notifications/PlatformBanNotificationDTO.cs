namespace WowsKarma.Common.Models.DTOs.Notifications;

public record PlatformBanNotificationDTO : NotificationBaseDTO
{
	public string Reason { get; set; } = string.Empty;
	public DateTimeOffset? Until { get; set; }
}
