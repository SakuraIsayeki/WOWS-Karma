using AngleSharp.Text;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.Components.Forms;
using Wargaming.WebAPI.Models;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Web.Services;
using WowsKarma.Web.Services.Authentication;

namespace WowsKarma.Web
{
	public static class Utilities
	{
		public static Region CurrentRegion { get; internal set; }

		public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
		{
			PropertyNameCaseInsensitive = true,
			NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
		};

		public static async Task<TResult> DeserializeFromHttpResponseAsync<TResult>(HttpResponseMessage response)
		{
			string json = await response.Content.ReadAsStringAsync();
			TResult result = JsonSerializer.Deserialize<TResult>(json, JsonSerializerOptions);
			return result;
		}

		public static string GetOidcEndpoint() => CurrentRegion switch
		{
			Region.EU => "https://eu.wargaming.net/id/",
			Region.NA => "https://na.wargaming.net/id/",
			Region.CIS => "https://ru.wargaming.net/id/",
			Region.ASIA => "https://asia.wargaming.net/id/",

			_ => throw new NotImplementedException()
		};

		public static string GetWowsNumbersBaseLink() => CurrentRegion switch
		{
			Region.EU => "https://wows-numbers.com/",
			Region.NA => "https://na.wows-numbers.com/",
			Region.CIS => "https://ru.wows-numbers.com/",
			Region.ASIA => "https://asia.wows-numbers.com/",

			_ => throw new NotImplementedException()
		};

		// Example Identifier:
		// https://eu.wargaming.net/id/0000000-JohnDoe/
		public static AccountListingDTO GetAccountInfoFromOidcUrl(string url)
		{
			Match result = new Regex("([0-9]+)-(\\w+)").Match(url);
			return new(result.Groups[1].Value.ToInteger(uint.MinValue), result.Groups[2].Value);
		}

		public static uint GetIdFromRouteParameter(string routeParameter)
		{
			Match result = new Regex("([0-9]+),(\\w+)").Match(routeParameter);
			return result.Groups[1].Value.ToInteger(uint.MinValue);
		}

		internal static string GetPostBorderColor(PostFlairs postFlairs)
		{
			return PostFlairsUtils.CountBalance(postFlairs.ParseFlairsEnum()) switch
			{
				> 0 => "success",
				< 0 => "danger",
				_ => "warning"
			};
		}

		internal static string GetKarmaColor(int karmaCount, int lowThreshold = 0, int highThreshold = 0) 
		{
			if (karmaCount < lowThreshold)
			{
				return "danger";
			}
			else if (karmaCount > highThreshold)
			{
				return "success";
			}

			return "warning";
		}

		internal static string GetTokenFromCookie(this HttpContext httpContext) => httpContext.User.FindFirstValue("token");

		internal static AuthenticationHeaderValue GenerateAuthenticationHeader(this HttpContext context) 
			=> new("Bearer", context.Request.Cookies[ApiTokenAuthenticationHandler.CookieName]);

		public static StreamContent AddReplayFile(this MultipartFormDataContent form, IBrowserFile replayFile, CancellationToken ct = default)
		{
			StreamContent fileContent = new(replayFile.OpenReadStream(ReplayService.MaxReplayFileSize, ct));
			fileContent.Headers.ContentDisposition = new("form-data")
			{
				Name = "replay",
				FileName = replayFile.Name
			};
			
			form.Add(fileContent, "replay", replayFile.Name);

			return fileContent; // This is a TERRIBLE hack. But at least it ensures no GC action before the replay is properly used.
		}
	}
}