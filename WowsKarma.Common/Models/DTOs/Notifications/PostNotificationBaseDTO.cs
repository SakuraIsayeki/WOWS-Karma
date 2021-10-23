namespace WowsKarma.Common.Models.DTOs.Notifications;

public record PostNotificationBaseDTO : NotificationBaseDTO
{
	public Guid PostId { get; init; }
	public PlayerPostDTO Post { get; init; }
}
