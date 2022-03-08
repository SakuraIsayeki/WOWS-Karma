using NodaTime;

namespace WowsKarma.Api.Data.Models
{
	public interface ITimestamped
	{
		public Instant CreatedAt { get; init; }
		public Instant UpdatedAt { get; set; }
	}
}
