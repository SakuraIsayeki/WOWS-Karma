using System.Security.Claims;
using Nodsoft.Wargaming.Api.Common;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Authentication.Wargaming;

public class WargamingIdentity : ClaimsIdentity
{
	public new const string AuthenticationType = "Wargaming";

	public WargamingIdentity(IEnumerable<Claim> claims) : base(claims, AuthenticationType) { }

	public static WargamingIdentity FromUri(Uri identityUri)
	{
		Region region = identityUri.Host.Split(".")[0].FromWargamingSubdomain();
		string segment = identityUri.Segments[^1];
		int index = segment.IndexOf('-');
		string accountId = segment[..index];
		string nickname = segment[(index + 1)..^1].Replace("/", string.Empty);

		List<Claim> claims = new()
		{
			new(ClaimTypes.NameIdentifier, accountId),
			new(ClaimTypes.Name, nickname),
			new(WargamingClaimTypes.Region, ((int)region).ToString()),
			new(WargamingClaimTypes.RegionName, region.ToString())
		};

		return new(claims);
	}

	public AccountListingDTO GetAccountListing()
	{
		if (uint.TryParse(Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier)?.Value, out uint id))
		{
			return new(id, Claims.FirstOrDefault(c => c.Type is ClaimTypes.Name)?.Value);
		}
			
		return null;
	}
}