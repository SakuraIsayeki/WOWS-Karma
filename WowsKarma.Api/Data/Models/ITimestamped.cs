namespace WowsKarma.Api.Data.Models
{
	public interface ITimestamped
	{
		public DateTime CreatedAt { get; init; }
		public DateTime UpdatedAt { get; set; }
	}
}
