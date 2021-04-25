using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wargaming.WebAPI.Models;

namespace WowsKarma.Common
{
	public static class Utilities
	{
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
	}
}
