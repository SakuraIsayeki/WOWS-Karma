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

	public Guid? ReplayId { get; init; }
	public ReplayDTO? Replay { get; init; }

	// Computed by DB Engine (hopefully)
	public DateTime? CreatedAt { get; init; }
	public DateTime? UpdatedAt { get; init; }
}
