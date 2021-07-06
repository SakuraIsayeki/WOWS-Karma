using System;
using System.ComponentModel.DataAnnotations;

namespace WowsKarma.Common.Models.DTOs
{
	public record PostModActionDTO
	{
		public Guid Id { get; init; }

		[Required]
		public Guid PostId { get; init; }

		public PlayerPostDTO UpdatedPost { get; init; }

		public ModActionType ActionType { get; init; }

		public uint ModId { get; init; }
		public uint ModUsername { get; init; }

		[Required]
		public string Reason { get; set; }
	}
}
