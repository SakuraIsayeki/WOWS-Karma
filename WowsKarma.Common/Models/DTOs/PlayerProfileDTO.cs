using System;



namespace WowsKarma.Common.Models.DTOs
{
	public record PlayerProfileDTO
	{
		public uint Id { get; init; }

		public string Username { get; init; }

		public int GameKarma { get; init; }
		public int SiteKarma { get; init; }

		public bool Hidden { get; init; }
		public bool OptedOut { get; init; }

		public int RatingPerformance { get; init; }
		public int RatingTeamplay { get; init; }
		public int RatingCourtesy { get; init; }

		public DateTime WgAccountCreatedAt { get; init; }
		public DateTime LastBattleTime { get; init; }
		public DateTime OptOutChanged { get; init; }

		public bool NegativeKarmaAble { get; init; }
		public bool PostsBanned { get; init; }
	}
}
