namespace WowsKarma.Common.Models.DTOs.Replays;

/// <remarks>
/// HACK: Fields are set as nullable to disable API validation.
/// </remarks>
public struct ReplayChatMessageDTO
{
	public uint? PlayerId { get; init; }
	public string? Username { get; init; }

	public string? MessageGroup { get; init; }

	public string? MessageContent { get; init; }
}
