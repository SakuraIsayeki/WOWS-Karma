namespace WowsKarma.Common.Models.DTOs.Replays;

public readonly struct ReplayChatMessageDTO
{
	public uint PlayerId { get; init; }
	public string Username { get; init; }

	public string MessageGroup { get; init; }

	public string MessageContent { get; init; }
}
