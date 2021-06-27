using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Wargaming.WebAPI.Models;

namespace WowsKarma.Common
{
	public static class Utilities
	{
		public static JsonSerializerOptions CookieSerializerOptions { get; } = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public static Region GetRegionConfigString(string configString) => configString switch
		{
			"EU" => Region.EU,
			"NA" => Region.NA,
			"CIS" or "RU" => Region.CIS,
			"ASIA" => Region.ASIA,
			_ => throw new ArgumentOutOfRangeException(nameof(configString))
		};

		public static string ToRegionString(this Region region) => region switch
		{
			Region.EU => "EU",
			Region.NA => "NA",
			Region.CIS => "CIS",
			Region.ASIA => "ASIA",
			_ => throw new ArgumentOutOfRangeException(nameof(region))
		};

		public static string ToWargamingSubdomain(this Region region) => region switch
		{
			Region.EU => "eu",
			Region.NA => "na",
			Region.CIS => "ru",
			Region.ASIA => "asia",
			_ => throw new ArgumentOutOfRangeException(nameof(region))
		};

		public static Region FromWargamingSubdomain(this string subdomain) => subdomain switch
		{
			"eu" => Region.EU,
			"na" => Region.NA,
			"ru" => Region.CIS,
			"asia" => Region.ASIA,
			_ => throw new ArgumentOutOfRangeException(nameof(subdomain))
		};

		public static string GetRegionWebDomain(this Region region) => region switch
		{
			Region.EU => "https://wows-karma.com/",
			Region.NA => "https://na.wows-karma.com/",
			Region.CIS => "https://ru.wows-karma.com/",
			Region.ASIA => "https://asia.wows-karma.com/",
			_ => throw new ArgumentOutOfRangeException(nameof(region))
		};

		public static string GetRegionApiDomain(this Region region) => region switch
		{
			Region.EU => "https://api.wows-karma.com/",
			Region.NA => "https://api.na.wows-karma.com/",
			Region.CIS => "https://api.ru.wows-karma.com/",
			Region.ASIA => "https://api.asia.wows-karma.com/",
			_ => throw new ArgumentOutOfRangeException(nameof(region))
		};

		public static string BuildQuery(params (string parameter, string value)[] arguments)
		{
			StringBuilder path = new();

			if (arguments is not null)
			{
				for (int i = 0; i < arguments.Length; i++)
				{
					path.AppendFormat("{0}{1}={2}", i is 0 ? '?' : '&', arguments[i].parameter, arguments[i].value);
				}
			}

			return path.ToString();
		}

		public static string BuildQuery(this IDictionary<string, string> arguments)
		{
			using IEnumerator<KeyValuePair<string, string>> enumerator = arguments.GetEnumerator();
			StringBuilder path = new();

			for (int i = 0; i < arguments.Count; i++)
			{
				KeyValuePair<string, string> current = enumerator.Current;
				path.AppendFormat("{0}{1}={2}", i is 0 ? '?' : '&', current.Key, current.Value);
				enumerator.MoveNext();
			}

			return path.ToString();
		}
	}
}
