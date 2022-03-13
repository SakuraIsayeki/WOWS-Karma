namespace WowsKarma.Common.Models.DTOs;

public record AccountListingDTO(uint Id, [Required(AllowEmptyStrings = true)] string Username);

public record AccountClanListingDTO(uint Id, [Required(AllowEmptyStrings = true)] string Username, ClanListingDTO? Clan) 
	: AccountListingDTO(Id, Username);
