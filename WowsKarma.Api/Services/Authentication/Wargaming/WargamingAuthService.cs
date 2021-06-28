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
		public static Uri OpenIdDomain { get; } = new($"https://{Startup.ApiRegion.ToWargamingSubdomain()}.wargaming.net/id/openid");

		private static string callbackUrl;

		private readonly HttpClient httpClient;

		public WargamingAuthService(IConfiguration configuration, ILogger<WargamingAuthService> logger, IHttpClientFactory httpClientFactory)
		{
			callbackUrl ??= configuration[$"Api:{Startup.ApiRegion.ToRegionString()}:WgAuthCallback"];
			httpClient = httpClientFactory.CreateClient();
		}

		public IActionResult RedirectToLogin(Region region, IDictionary<string, string> extraRedirectParams = null) => new RedirectResult(GetAuthUri(region, extraRedirectParams).ToString());

		public Uri GetAuthUri(Region region, IDictionary<string, string> extraRedirectParams = null)
		{
			string verifyIdentityUri = callbackUrl;

			if (extraRedirectParams?.Any() is true)
			{
				string queryString = string.Join('&', extraRedirectParams
					.Where(e => !string.IsNullOrEmpty(e.Value))
					.Select(param => $"{param.Key}={param.Value}"));

				verifyIdentityUri += $"?{queryString}";
			}

			UriBuilder builder = new(OpenIdDomain);

			builder.Query = BuildQuery(
				("openid.ns", "http://specs.openid.net/auth/2.0"),
				("openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select"),
				("openid.identity", "http://specs.openid.net/auth/2.0/identifier_select"),
				("openid.return_to", verifyIdentityUri),
				("openid.mode", "checkid_setup")
			);

			return builder.Uri;
		}

		
		/*
		 * FIXME
		 */
		public async Task<bool> IsValid(Dictionary<string, string> paramDict)
		{
			paramDict.Remove("redirectUri");
			paramDict["openid.mode"] = "check_authentication";


			using HttpResponseMessage response = await httpClient.PostAsync(paramDict.BuildQuery(), null);
			string content = await response.Content.ReadAsStringAsync();
			return (content).Contains("is_valid:true");
		}
	}
}
