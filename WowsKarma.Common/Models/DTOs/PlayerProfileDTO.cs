using System;



namespace WowsKarma.Common.Models.DTOs
{
	public record PlayerProfileDTO
	{
		public PlayerProfileDTO() { }
		public PlayerProfileDTO(PlayerProfileDTO source)
		{
			Id = source.Id;
			Username = source.Username;
			WgKarma = source.WgKarma;
			WgAccountCreatedAt = source.WgAccountCreatedAt;
			LastBattleTime = source.LastBattleTime;
		}


		public uint Id { get; init; }

		public string Username { get; init; }

		public int WgKarma { get; init; }

		public DateTime WgAccountCreatedAt { get; init; }
		public DateTime LastBattleTime { get; init; }
	}
}
