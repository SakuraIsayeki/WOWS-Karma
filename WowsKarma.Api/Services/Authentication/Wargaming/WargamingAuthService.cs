using Microsoft.AspNetCore.Mvc;
using WowsKarma.Common;

using static WowsKarma.Common.Utilities;

namespace WowsKarma.Api.Services.Authentication.Wargaming;

public sealed class WargamingAuthService
{
	private static readonly Uri _openIdDomain = new($"https://{Startup.ApiRegion.ToWargamingSubdomain()}.wargaming.net/id/openid");

	private static string? _callbackUrl;

	private readonly WargamingAuthClientFactory _authClientFactory;

	public WargamingAuthService(IConfiguration configuration, WargamingAuthClientFactory authClientFactory)
	{
		_callbackUrl ??= configuration[$"Api:{Startup.ApiRegion.ToRegionString()}:WgAuthCallback"];
		_authClientFactory = authClientFactory;
	}

	public static IActionResult RedirectToLogin(IReadOnlyDictionary<string, string?>? extraRedirectParams = null) => new RedirectResult(GetAuthUri(extraRedirectParams).ToString());

	public static Uri GetAuthUri(IReadOnlyDictionary<string, string?>? extraRedirectParams = null)
	{
		string verifyIdentityUri = _callbackUrl!;

		if (extraRedirectParams is { Count: not 0 })
		{
			string queryString = string.Join('&', 
				from e in extraRedirectParams
				where e is { Value: not (null or "") }
				select $"{e.Key}={e.Value}");

			verifyIdentityUri += $"?{queryString}";
		}

		UriBuilder builder = new(_openIdDomain)
		{
			Query = BuildQuery(
				("openid.ns", "http://specs.openid.net/auth/2.0"),
				("openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select"),
				("openid.identity", "http://specs.openid.net/auth/2.0/identifier_select"),
				("openid.return_to", verifyIdentityUri),
				("openid.mode", "checkid_setup")
			)
		};

		return builder.Uri;
	}


	public async Task<bool> VerifyIdentity(HttpRequest context)
	{
		// https://eu.wargaming.net/id/503276471-cpt_stewie/

		Dictionary<string, string?> paramDict = context.Query.ToDictionary(kv => kv.Key, kv => kv.Value.FirstOrDefault());
		bool isValid = await IsValid(paramDict);

		return isValid;
	}

	private async Task<bool> IsValid(IDictionary<string, string?> paramDict)
	{
		paramDict["openid.mode"] = "check_authentication";


		using HttpClient httpClient = _authClientFactory.GetClient(Startup.ApiRegion);
		using HttpResponseMessage response = await httpClient.PostAsync($"id/openid{paramDict.BuildQuery()}", null);
		string stringResponse = await response.Content.ReadAsStringAsync();
		return stringResponse.Contains("is_valid:true");
	}
}