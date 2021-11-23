namespace WowsKarma.Common.Models.DTOs.Notifications;

public record PlatformBanNotificationDTO : NotificationBaseDTO
{
	public string Reason { get; set; }
	public DateTime? Until { get; set; }
}
