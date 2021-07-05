using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WowsKarma.Common.Models;

namespace WowsKarma.Api.Data.Models
{
	public record PostModAction
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; init; }

		public Post Post { get; init; }
		public Guid PostId { get; init; }



		public ModActionType ActionType { get; init; }

		public Player Mod { get; init; }
		public uint ModId { get; init; }

		public string Reason { get; set; }
	}
}
