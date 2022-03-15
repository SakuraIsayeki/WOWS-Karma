using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text.Json.Serialization;

namespace WowsKarma.Common.Models.DTOs;

public record ClanListingDTO
{
	public uint Id { get; init; }

	public string Tag { get; init; } = string.Empty;
	public string Name { get; init; } = string.Empty;

	public uint LeagueColor { get; init; }

	[JsonIgnore, NotMapped]
	public string LeagueColorHex => LeagueColor.ToString("X")[2..8];
}