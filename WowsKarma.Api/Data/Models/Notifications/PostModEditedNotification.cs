using Mapster;
using WowsKarma.Common.Models.DTOs.Notifications;

namespace WowsKarma.Api.Data.Models.Notifications;

public sealed record PostModEditedNotification : NotificationBase, IDisposable
{
	public override NotificationType Type { get; private protected init; } = NotificationType.PostModEdited;

	public Guid ModActionId { get; set; }
	public PostModAction ModAction { get; set; } = null!;


	public static PostModDeletedNotification FromModAction(PostModAction modAction) => modAction?.ActionType is not ModActionType.Deletion
		? throw new ArgumentException(null, nameof(modAction))
		: new()
		{
			AccountId = modAction.Post.AuthorId,
			Account = modAction.Post.Author,
			ModActionId = modAction.Id,
			ModAction = modAction
		};

	public override PostModEditedNotificationDTO ToDTO() => new()
	{
		Id = Id,
		AccountId = AccountId,
		AcknowledgedAt = AcknowledgedAt,
		EmittedAt = EmittedAt,
		ModAction = ModAction.Adapt<PostModActionDTO>(),
		ModActionId = ModActionId,
		Type = Type
	};

	public void Dispose()
	{
		ModAction.Dispose();
	}
}