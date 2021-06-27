using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Wargaming.WebAPI.Models;
using WowsKarma.Common;

using static WowsKarma.Common.Utilities;



namespace WowsKarma.Api.Services.Authentication.Wargaming
{
	internal class WargamingAuthConfig
	{
		public string VerifyIdentityUri { get; set; }
	}

	public class WargamingAuthService
	{
		private static readonly WargamingAuthConfig config = new();

		public WargamingAuthService(IConfiguration configuration, ILogger<WargamingAuthService> logger)
		{
			config.VerifyIdentityUri ??= configuration[$"Api:{Startup.ApiRegion.ToRegionString()}:WgAuthCallback"];
		}

		public IActionResult RedirectToLogin(Region region, Dictionary<string, string> extraRedirectParams = null) => new RedirectResult(GetAuthUri(region, extraRedirectParams).ToString());

		public Uri GetAuthUri(Region region, Dictionary<string, string> extraRedirectParams = null)
		{
			string verifyIdentityUri = config.VerifyIdentityUri;

			if (extraRedirectParams?.Any() is true)
			{
				string queryString = string.Join('&', extraRedirectParams
					.Where(e => !string.IsNullOrEmpty(e.Value))
					.Select(param => $"{param.Key}={param.Value}"));

				verifyIdentityUri += $"?{queryString}";
			}
			string subDomain = region.ToWargamingSubdomain();
			UriBuilder builder = new($"https://{subDomain}.wargaming.net/id/openid");

			builder.Query = BuildQuery(
				("openid.ns", "http://specs.openid.net/auth/2.0"),
				("openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select"),
				("openid.identity", "http://specs.openid.net/auth/2.0/identifier_select"),
				("openid.return_to", verifyIdentityUri),
				("openid.mode", "checkid_setup")
			);

			return builder.Uri;
		}

		public static async Task<(bool, ClaimsPrincipal)> VerifyIdentity(HttpRequest context)
		{
			//https://eu.wargaming.net/id/503276471-cpt_stewie/

			Dictionary<string, string> paramDict = context.Query.ToDictionary(kv => kv.Key, kv => kv.Value.FirstOrDefault());
			Uri identityUri = new(paramDict["openid.identity"]);
			bool isValid = await IsValid(paramDict, identityUri);

			if (!isValid)
			{
				return (false, null);
			}

			return (true, new ClaimsPrincipal(WargamingIdentity.FromUri(identityUri)));
		}

		private static async Task<bool> IsValid(Dictionary<string, string> paramDict, Uri identityUri)
		{
			paramDict["openid.mode"] = "check_authentication";

			using HttpClient httpClient = new() { BaseAddress = new UriBuilder(identityUri.Scheme, identityUri.Host).Uri };
			using HttpResponseMessage response = await httpClient.PostAsync("id/openid" + paramDict.BuildQuery(), null);
			return (await response.Content.ReadAsStringAsync()).Contains("is_valid:true");
		}
	}
}
