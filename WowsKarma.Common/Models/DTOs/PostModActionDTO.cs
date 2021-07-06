using System;

namespace WowsKarma.Common.Models.DTOs
{
	public record PostModActionDTO
	{
		public Guid Id { get; init; }

		public Guid PostId { get; init; }
		public PlayerPostDTO UpdatedPost { get; init; }

		public ModActionType ActionType { get; init; }

		public uint ModId { get; init; }
		public uint ModUsername { get; init; }

		public string Reason { get; set; }
	}
}
