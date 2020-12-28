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
	}
}
