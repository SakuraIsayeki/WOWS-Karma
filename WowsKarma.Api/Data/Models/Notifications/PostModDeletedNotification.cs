using System;

namespace WowsKarma.Api.Data.Models.Notifications;

public record PostModDeletedNotification : NotificationBase
{
	public override NotificationType Type { get; protected private init; } = NotificationType.PostModDeleted;

	public virtual Guid ModActionId { get; set; }
	public virtual PostModAction ModAction { get; set; }


	public static PostModDeletedNotification FromModAction(PostModAction modAction) => modAction?.ActionType is not ModActionType.Deletion
		? throw new ArgumentException(null, nameof(modAction))
		: new()
		{
			AccountId = modAction.Post.AuthorId,
			Account = modAction.Post.Author,
			ModActionId = modAction.Id,
			ModAction = modAction
		};
}
