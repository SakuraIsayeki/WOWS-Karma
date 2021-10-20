namespace WowsKarma.Common.Models.DTOs.Notifications;

public record PostModDeletedNotificationDTO : NotificationDTO
{
	public virtual Guid ModActionId { get; set; }
	public virtual PostModActionDTO ModAction { get; set; }
}
