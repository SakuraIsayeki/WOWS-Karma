using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WowsKarma.Api.Data.Models.Replays;

namespace WowsKarma.Api.Data.Models
{
	public record Post : ITimestamped
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; init; }

		[Required]
		public uint PlayerId { get; init; }
		[Required]
		public Player Player { get; init; }
		[Required]
		public uint AuthorId { get; init; }
		[Required]
		public Player Author { get; init; }

		public PostFlairs Flairs { get; set; }
		public PostFlairsParsed ParsedFlairs => Flairs.ParseFlairsEnum();

		[Required]
		public string Title { get; set; }
		[Required]
		public string Content { get; set; }

		public Guid? ReplayId { get; set; }
		public virtual Replay Replay { get; set; }

		// Computed by DB Engine (hopefully)
		public DateTime CreatedAt { get; init; }
		public DateTime UpdatedAt { get; set; }

		public bool NegativeKarmaAble { get; internal set; }

		public bool ModLocked { get; set; }
	}
}
