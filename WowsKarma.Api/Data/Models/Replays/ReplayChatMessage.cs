namespace WowsKarma.Api.Data.Models.Replays;

public readonly struct ReplayChatMessage
{
	public uint PlayerId { get; init; }

	public uint EntityId { get; init; }
	public string MessageGroup { get; init; }
	public string MessageContent { get; init; }
}
