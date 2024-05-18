using Mapster;
using WowsKarma.Common.Models.DTOs.Notifications;

namespace WowsKarma.Api.Data.Models.Notifications;

/// <summary>
/// Represents a notification that a post was deleted by a moderator.
/// </summary>
public sealed record PostModDeletedNotification : NotificationBase, IDisposable
{
	/// <inheritdoc />
	public override NotificationType Type { get; private protected init; } = NotificationType.PostModDeleted;

	/// <summary>
	/// The unique identifier of the moderation action that deleted the post.
	/// </summary>
	public Guid ModActionId { get; set; }
	
	/// <summary>
	/// The moderation action that deleted the post.
	/// </summary>
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

	/// <inheritdoc />
	public override PostModDeletedNotificationDTO ToDTO() => new()
	{
		Id = Id,
		AccountId = AccountId,
		AcknowledgedAt = AcknowledgedAt,
		EmittedAt = EmittedAt,
		ModAction = ModAction.Adapt<PostModActionDTO>(),
		ModActionId = ModActionId,
		Type = Type
	};

	/// <inheritdoc />
	public void Dispose()
	{
		ModAction.Dispose();
	}
}
