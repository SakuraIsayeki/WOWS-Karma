using WowsKarma.Common.Models.DTOs.Replays;

namespace WowsKarma.Common.Models.DTOs;

public record PlayerPostDTO
{
	public Guid? Id { get; init; }

	public AccountClanListingDTO Player { get; init; }
	
	public AccountClanListingDTO Author { get; init; }

	public PostFlairs Flairs { get; init; }

	public string Title { get; init; } = string.Empty;
	public string Content { get; init; } = string.Empty;

	public bool ModLocked { get; init; }
	public bool ReadOnly { get; init; }

	public Guid? ReplayId { get; init; }
	public ReplayDTO? Replay { get; init; }
    
	public ReplayState ReplayState { get; init; }

	// Computed by DB Engine (hopefully)
	public DateTimeOffset? CreatedAt { get; init; }
	public DateTimeOffset? UpdatedAt { get; init; }
	
	/// <summary>
	/// The status of the Customer Support ticket associated with the post when applicable.
	/// </summary>
	public CustomerSupportStatus SupportTicketStatus { get; init; }
	
	/// <summary>
	/// Defines the status of the Customer Support ticket associated with the post when applicable.
	/// </summary>
	public readonly struct CustomerSupportStatus
	{
		/// <summary>
		/// Whether the post has an associated Customer Support ticket.
		/// </summary>
		public bool HasTicket { get; init; }
		
		/// <summary>
		/// The Customer Support ticket ID associated with this post.
		/// </summary>
		/// <remarks>
		/// This is only available when <see cref="HasTicket"/> is <see langword="true"/>,
		/// and only visible to the post's author, platform staff, and Wargaming staff.
		/// </remarks>
		public int? TicketId { get; init; }
	}
}
