using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WowsKarma.Api.Data.Models.Auth;

public sealed record Role
{
	[Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public byte Id { get; init; }

	[Required]
	public string InternalName { get; init; } = "";

	[Required]
	public string DisplayName { get; set; } = "";

	public List<User> Users { get; set; } = [];
}