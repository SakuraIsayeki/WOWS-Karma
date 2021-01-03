using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WowsKarma.Api.Utilities;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Data.Models
{
	public record Player : IDataModel<uint>, ITimestamped
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public uint Id { get; init; }

		public DateTime CreatedAt { get; init; }
		public DateTime UpdatedAt { get; set; }

		public string Username { get; set; }

		public int SiteKarma { get; set; }
		public int GameKarma { get; set; }

		public DateTime WgAccountCreatedAt { get; init; }
		public DateTime LastBattleTime { get; set; }

		public List<Post> PostsReceived { get; init; }
		public List<Post> PostsSent { get; init; }


		public bool NegativeKarmaAble => (SiteKarma + GameKarma) > -20;


		/*
		 * Mapping
		 */

		public static implicit operator PlayerProfileDTO(Player value) => new()
		{
			Id = value.Id,
			Username = value.Username,
			WgAccountCreatedAt = value.WgAccountCreatedAt,
			WgKarma = value.GameKarma,
			LastBattleTime = value.LastBattleTime
		};

		public static Player Map(Player source, Player mod)
		{
			source.Username = mod.Username;
			source.GameKarma = mod.GameKarma;
			source.LastBattleTime = mod.LastBattleTime;

			return source;
		}
	}
}
