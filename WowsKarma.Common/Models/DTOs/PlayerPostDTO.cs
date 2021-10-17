namespace WowsKarma.Common.Models.DTOs;

public record PlayerPostDTO
{
	public Guid? Id { get; init; }

	public uint PlayerId { get; init; }
	public string PlayerUsername { get; init; }
	public uint AuthorId { get; init; }
	public string AuthorUsername { get; init; }

	public PostFlairs Flairs { get; init; }

	public string Title { get; init; }
	public string Content { get; init; }

	public bool ModLocked { get; init; }

	// Computed by DB Engine (hopefully)
	public DateTime? CreatedAt { get; init; }
	public DateTime? UpdatedAt { get; init; }
}
