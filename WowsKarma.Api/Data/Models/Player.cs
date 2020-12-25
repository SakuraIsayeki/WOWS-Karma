using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Data.Models
{
	public record Player : IDataModel<uint>
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public uint Id { get; init; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime LastUpdated { get; set; }

		public string Username { get; set; }

		public int WgKarma { get; set; }

		public DateTime WgAccountCreatedAt { get; init; }

		public IEnumerable<Post> PostsReceived { get; init; }
		public IEnumerable<Post> PostsSent { get; init; }

		public static implicit operator PlayerProfileDTO(Player value) => new()
		{
			Id = value.Id,
			Username = value.Username,
			WgAccountCreatedAt = value.WgAccountCreatedAt,
			WgKarma = value.WgKarma
		};
	}
}
