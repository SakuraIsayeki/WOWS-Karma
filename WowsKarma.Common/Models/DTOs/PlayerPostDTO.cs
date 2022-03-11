﻿using WowsKarma.Common.Models.DTOs.Replays;

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

	public Guid? ReplayId { get; init; }
	public ReplayDTO Replay { get; init; }

	// Computed by DB Engine (hopefully)
	public DateTimeOffset? CreatedAt { get; init; }
	public DateTimeOffset? UpdatedAt { get; init; }
}
