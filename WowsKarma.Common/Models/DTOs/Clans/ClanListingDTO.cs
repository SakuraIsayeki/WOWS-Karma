using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text.Json.Serialization;

namespace WowsKarma.Common.Models.DTOs;

public record ClanListingDTO
{
	public uint Id { get; init; }

	public string Tag { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;

	public uint LeagueColor { get; set; }

	[JsonIgnore, NotMapped]
	public string LeagueColorHex => LeagueColor.ToString("X")[2..8];
}