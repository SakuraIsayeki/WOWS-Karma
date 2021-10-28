using Mapster;
using WowsKarma.Common.Models.DTOs.Notifications;

namespace WowsKarma.Api.Data.Models.Notifications;

public abstract record PostNotificationBase : NotificationBase
{
	public virtual Guid PostId { get; init; }
	public virtual Post Post { get; init; }



	public override NotificationBaseDTO ToDTO() => new PostAddedNotificationDTO
	{
		Id = Id,
		Post = Post.Adapt<PlayerPostDTO>(),
		PostId = PostId,
		AccountId = AccountId,
		AcknowledgedAt = AcknowledgedAt,
		EmittedAt = EmittedAt,
		Type = Type
	};
}
