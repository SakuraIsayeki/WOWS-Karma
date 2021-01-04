using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowsKarma.Common.Models.DTOs
{
	public record PlayerPostDTO
	{
		public Guid? Id { get; init; }

		public uint PlayerId { get; init; }
		public uint AuthorId { get; init; }

		public PostFlairs Flairs { get; init; }

		public string Title { get; init; }
		public string Content { get; init; }

		// Computed by DB Engine (hopefully)
		public DateTime? PostedAt { get; init; }
		public DateTime? UpdatedAt { get; init; }
	}
}
