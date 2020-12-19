using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace WowsKarma.Api.Data.Models
{
	public record Player : IDataModel<uint>
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public uint Id { get; init; }

		public string Username { get; set; }

		public int WgKarma { get; set; }

		public DateTime WgAccountCreatedAt { get; init; }

		public IEnumerable<Post> PostsReceived { get; init; }
		public IEnumerable<Post> PostsSent { get; init; }
	}
}
