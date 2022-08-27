using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WowsKarma.Api.Data.Models.Auth;

public record Role
{
	[Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public byte Id { get; init; }

	[Required]
	public string InternalName { get; init; }

	[Required]
	public string DisplayName { get; set; }

	public IEnumerable<User> Users { get; set; }
}