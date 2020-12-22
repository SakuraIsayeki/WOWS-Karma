using System;



namespace WowsKarma.Common.Models.DTOs
{
	public record PlayerProfileDTO
	{
		public uint Id { get; init; }

		public string Username { get; init; }

		public int WgKarma { get; init; }

		public DateTime WgAccountCreatedAt { get; init; }
	}
}
