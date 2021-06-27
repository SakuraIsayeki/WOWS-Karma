using System;
using System.Collections.Generic;
using System.Security.Claims;
using Wargaming.WebAPI.Models;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Authentication.Wargaming
{
	public class WargamingIdentity : ClaimsIdentity
	{
		

		public WargamingIdentity(IEnumerable<Claim> claims) : base(claims, "Wargaming") { }

		public static WargamingIdentity FromUri(Uri identityUri)
		{
			Region region = identityUri.Host.Split(".")[0].FromWargamingSubdomain();


			string segment = identityUri.Segments[^1];
			int index = segment.IndexOf('-');
			string accountId = segment[..index];
			string nickname = segment[(index + 1)..^1].Replace("/", string.Empty);

			List<Claim> claims = new()
			{
				new Claim(ClaimTypes.NameIdentifier, accountId),
				new Claim(ClaimTypes.Name, nickname),
				new Claim(WargamingClaimTypes.Region, ((int)region).ToString()),
				new Claim(WargamingClaimTypes.RegionName, region.ToString())
			};

			return new WargamingIdentity(claims);
		}

	}
}
