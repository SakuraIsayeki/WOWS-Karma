namespace WowsKarma.Api.Data.Models;

public interface ITimestamped
{
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset UpdatedAt { get; set; }
}