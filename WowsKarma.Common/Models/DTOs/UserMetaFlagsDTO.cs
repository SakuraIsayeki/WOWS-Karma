﻿namespace WowsKarma.Common.Models.DTOs
{
	public record UserMetaFlagsDTO
	{
		public uint Id { get; init; }

		public bool PostsBanned { get; init; }
		public bool OptedOut { get; init; }
	}
}
