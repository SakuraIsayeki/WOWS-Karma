using System;



namespace WowsKarma.Common.Models.DTOs
{
	public record PlayerProfileDTO
	{
		public PlayerProfileDTO() { }
		public PlayerProfileDTO(PlayerProfileDTO from)
		{
			Id = from.Id;
			Username = from.Username;
			WgKarma = from.WgKarma;
			WgAccountCreatedAt = from.WgAccountCreatedAt;
		}


		public uint Id { get; init; }

		public string Username { get; init; }

		public int WgKarma { get; init; }

		public DateTime WgAccountCreatedAt { get; init; }
	}
}
