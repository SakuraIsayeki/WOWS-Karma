﻿namespace WowsKarma.Api.Data.Models.Replays;

public readonly struct ReplayPlayer
{
	public uint Id { get; init; }
	public uint AvatarId { get; init; }

	public uint AccountId { get; init; }
	public string Name { get; init; }

	public uint? ClanId { get; init; }
	public string? ClanTag { get; init; }

	public uint TeamId { get; init; }

	public uint ShipId { get; init; }

	public uint ShipParamsId { get; init; }
}
