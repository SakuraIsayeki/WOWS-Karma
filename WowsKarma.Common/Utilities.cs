using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Nodsoft.Wargaming.Api.Common;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Common;


public static class Utilities
{
	public static JsonSerializerOptions ApiSerializerOptions { get; } = new()
	{
		PropertyNameCaseInsensitive = true,
		NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
	};

	public static JsonSerializerOptions CookieSerializerOptions { get; } = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};
	
	[Pure] 
	public static Region GetRegionConfigString(string configString) => configString switch
	{
		"EU" => Region.EU,
		"NA" => Region.NA,
		"CIS" or "RU" => Region.CIS,
		"SEA" => Region.SEA,
		_ => throw new ArgumentOutOfRangeException(nameof(configString))
	};

	[Pure] 
	public static string ToRegionString(this Region region) => region switch
	{
		Region.EU => "EU",
		Region.NA => "NA",
		Region.CIS => "CIS",
		Region.SEA => "SEA",
		_ => throw new ArgumentOutOfRangeException(nameof(region))
	};

	[Pure] 
	public static string ToWargamingSubdomain(this Region region) => region switch
	{
		Region.EU => "eu",
		Region.NA => "na",
		Region.CIS => "ru",
		Region.SEA => "asia",
		_ => throw new ArgumentOutOfRangeException(nameof(region))
	};

	[Pure] 
	public static Region FromWargamingSubdomain(this string? subdomain) => subdomain switch
	{
		"eu" => Region.EU,
		"na" => Region.NA,
		"ru" => Region.CIS,
		"asia" => Region.SEA,
		_ => throw new ArgumentOutOfRangeException(nameof(subdomain))
	};

	[Pure] 
	public static string GetRegionWebDomain(this Region region) => region switch
	{
		Region.EU => "https://wows-karma.com/",
		Region.NA => "https://na.wows-karma.com/",
		Region.CIS => "https://ru.wows-karma.com/",
		Region.SEA => "https://asia.wows-karma.com/",
		_ => throw new ArgumentOutOfRangeException(nameof(region))
	};

	[Pure] 
	public static string GetRegionApiDomain(this Region region) => region switch
	{
		Region.EU => "https://api.wows-karma.com/",
		Region.NA => "https://api.na.wows-karma.com/",
		Region.CIS => "https://api.ru.wows-karma.com/",
		Region.SEA => "https://api.asia.wows-karma.com/",
		_ => throw new ArgumentOutOfRangeException(nameof(region))
	};

	[Pure] 
	public static string BuildQuery(params (string parameter, string value)[] arguments)
	{
		StringBuilder path = new();

		for (int i = 0; i < arguments.Length; i++)
		{
			path.Append($"{(i is 0 ? '?' : '&')}{arguments[i].parameter}={arguments[i].value}");
		}

		return path.ToString();
	}

	[Pure] 
	public static string BuildQuery(this IDictionary<string, string?> arguments)
	{
		using IEnumerator<KeyValuePair<string, string?>> enumerator = arguments.GetEnumerator();
		StringBuilder path = new();

		for (int i = 0; i < arguments.Count; i++)
		{
			enumerator.MoveNext();
			if (enumerator.Current is (var key, { } value))
			{
				path.Append($"{(i is 0 ? '?' : '&')}{key}={Uri.EscapeDataString(value)}");
			}
		}

		return path.ToString();
	}
	
	public static AccountListingDTO? ToAccountListing(this ClaimsPrincipal? claimsPrincipal) 
		=> uint.TryParse(claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out uint accountId) 
			? new AccountListingDTO(accountId, claimsPrincipal.FindFirst(ClaimTypes.Name)!.Value) 
			: null;


	[Pure]
	public static Type? GetType(string typeName)
	{
		if (Type.GetType(typeName) is { } type)
		{
			return type;
		}

		foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
		{
			if ((type = a.GetType(typeName)) is not null)
			{
				return type;
			}
		}

		return null;
	}

	[Pure]
	public static ReplayChatMessageChannel GetMessageChannelType(string messageGroup) => messageGroup switch
	{
		"battle_common" => ReplayChatMessageChannel.All,
		"battle_team" => ReplayChatMessageChannel.Team,
		"battle_prebattle" => ReplayChatMessageChannel.Division,
		_ => ReplayChatMessageChannel.Unknown
	};

	[Pure]
	public static string GetDisplayString(this ReplayChatMessageChannel channel) => channel switch
	{
		ReplayChatMessageChannel.All => "All",
		ReplayChatMessageChannel.Team => "Team",
		ReplayChatMessageChannel.Division => "Division",
		_ => "Unknown"
	};
}
