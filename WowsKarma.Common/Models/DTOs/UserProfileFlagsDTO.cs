namespace WowsKarma.Common.Models.DTOs;

public sealed record UserProfileFlagsDTO
{
	public uint Id { get; init; }

	public bool PostsBanned { get; init; }

	public bool OptedOut { get; init; }
	public DateTimeOffset OptOutChanged { get; init; }

	public IEnumerable<byte> ProfileRoles { get; init; } = [];
}
