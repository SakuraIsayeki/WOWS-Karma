using Mapster;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Web.Models;

namespace WowsKarma.Web.Infrastructure;

public static class Conversions
{
	public static void ConfigureMapping()
	{
		TypeAdapterConfig<PlayerPostViewModel, PlayerPostDTO>
			.NewConfig()
			.IgnoreNullValues(true)
			.Map(dest => dest.Author, src => new AccountClanListingDTO(src.AuthorId, "", null))
			.Map(dest => dest.Player, src => new AccountClanListingDTO(src.PlayerId, "", null));
	}
}