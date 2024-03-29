﻿namespace WowsKarma.Common.Models.DTOs.Replays;

public sealed record ReplayDTO
{
	public Guid Id { get; init; }

	public Guid PostId { get; init; }

	public string DownloadUri { get; init; } = "";
	
	public string? MinimapUri { get; init; }

	public IEnumerable<ReplayPlayerDTO> Players { get; set; } = [];

	public IEnumerable<ReplayChatMessageDTO> ChatMessages { get; set; } = [];
}
