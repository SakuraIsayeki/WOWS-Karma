namespace WowsKarma.Common.Models.DTOs;

public record PostModActionDTO
{
	public Guid Id { get; init; }

	[Required]
	public Guid PostId { get; init; }

	public PlayerPostDTO? UpdatedPost { get; init; }

	public ModActionType ActionType { get; init; }

	public uint ModId { get; init; }
	public string ModUsername { get; init; } = string.Empty;

	[Required]
	public string Reason { get; set; } = string.Empty;
}
