namespace WowsKarma.Api.Data.Models.Auth
{
	public record Role
	{
		public byte Id { get; init; }

		public string InternalName { get; init; }

		public string DisplayName { get; set; }
	}
}