using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Api.Data.Models
{
	public record Post : IDataModel<Guid>
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; init; }

		public Player Player { get; init; }
		public Player Author { get; init; }

		public PostKarmaTypes PositiveKarma { get; set; }
		public PostKarmaTypes NegativeKarma { get; set; }

		public string Title { get; set; }
		public string Content { get; set; }
	}

	[Flags]
	public enum PostKarmaTypes
	{
		Courtesy = 0x1,
		Performance = 0x2,
		Teamwork = 0x4
	}
}
