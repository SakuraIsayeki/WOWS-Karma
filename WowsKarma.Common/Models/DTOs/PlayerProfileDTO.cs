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
			GameKarma = source.GameKarma;
			SiteKarma = source.SiteKarma;
			RatingPerformance = source.RatingPerformance;
			RatingTeamplay = source.RatingTeamplay;
			RatingCourtesy = source.RatingCourtesy;
			WgAccountCreatedAt = source.WgAccountCreatedAt;
			LastBattleTime = source.LastBattleTime;
		}


		public uint Id { get; init; }

		public string Username { get; init; }

		public int GameKarma { get; init; }
		public int SiteKarma { get; init; }

		public int RatingPerformance { get; init; }
		public int RatingTeamplay { get; init; }
		public int RatingCourtesy { get; init; }

		public DateTime WgAccountCreatedAt { get; init; }
		public DateTime LastBattleTime { get; init; }
	}
}
