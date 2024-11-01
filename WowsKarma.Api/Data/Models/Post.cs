using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WowsKarma.Api.Data.Models.Replays;

namespace WowsKarma.Api.Data.Models;

/// <summary>
/// Represents a player post, sent by one player to another.
/// </summary>
public sealed record Post : ITimestamped, IDisposable
{
	/// <summary>
	/// The unique identifier of the post.
	/// </summary>
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	/// <summary>
	/// The unique identifier of the player who received the post.
	/// </summary>
	[Required] public uint PlayerId { get; init; }
	
	/// <summary>
	/// The player who received the post.
	/// </summary>
	[Required] public Player Player { get; init; } = null!;
	
	/// <summary>
	/// The unique identifier of the player who sent the post.
	/// </summary>
	[Required] public uint AuthorId { get; init; }
	
	/// <summary>
	/// The player who sent the post.
	/// </summary>
	[Required] public Player Author { get; init; } = null!;

	/// <summary>
	/// The unique identifier of the post's parent post.
	/// </summary>
	public PostFlairs Flairs { get; set; }
	
	/// <summary>
	/// The parsed flairs of the post.
	/// </summary>
	public PostFlairsParsed? ParsedFlairs => Flairs.ParseFlairsEnum();

	/// <summary>
	/// The title of the post.
	/// </summary>
	[Required] public string Title { get; set; } = "";
	
	/// <summary>
	/// The content of the post.
	/// </summary>
	[Required] public string Content { get; set; } = "";

	/// <summary>
	/// The unique identifier of the replay associated with the post.
	/// </summary>
	public Guid? ReplayId { get; set; }
	
	/// <summary>
	/// The replay associated with the post.
	/// </summary>
	public Replay? Replay { get; set; }

	// Computed by DB Engine (hopefully)
	
	/// <summary>
	/// The date and time the post was created.
	/// </summary>
	public DateTimeOffset CreatedAt { get; init; }
	
	/// <summary>
	/// The date and time the post was last updated.
	/// </summary>
	public DateTimeOffset UpdatedAt { get; set; }

	/// <summary>
	/// Whether this post is able to negatively affect the receiving player's karma.
	/// </summary>
	public bool NegativeKarmaAble { get; internal set; }

	/// <summary>
	/// Whether this post is locked for edits.
	/// </summary>
	public bool ReadOnly { get; set; }
	
	/// <summary>
	/// Whether this post was locked by mods, and inaccessible on the platform.
	/// </summary>
	public bool ModLocked { get; set; }

	/// <summary>
	/// The Customer Support ticket ID associated with this post.
	/// </summary>
	/// <remarks>
	/// This is used by WG staff to track and manage player reports.
	/// </remarks>
	/// <value>
	/// Set as <see langword="null"/> if no ticket is associated.
	/// When present, takes form as a positive integer, up to 9 digits.
	/// </value>
	[Range(1, 999_999_999)]
	public int? CustomerSupportTicketId { get; set; }
	
	/// <inheritdoc />
	public void Dispose()
	{
		Replay?.Dispose();
	}
}