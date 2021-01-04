using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WowsKarma.Common.Models
{
	[Flags]
	public enum PostFlairs : ushort
	{
		Neutral			= 0x00,

		PerformanceGood	= 0x01,
		PerformanceBad	= 0x02,

		TeamplayGood	= 0x04,
		TeamplayBad		= 0x08,

		CourtesyGood	= 0x10,
		CourtesyBad		= 0x20
	}

	public record PostFlairsParsed(bool? Performance, bool? Teamplay, bool? Courtesy);


	public static class PostFlairsUtils 
	{
		public static PostFlairs SanitizeFlairs(this PostFlairs flairs)
		{
			if (flairs is not 0)
			{
				RemoveConflictingFlags(flairs, PostFlairs.PerformanceGood, PostFlairs.PerformanceBad);
				RemoveConflictingFlags(flairs, PostFlairs.TeamplayGood, PostFlairs.TeamplayBad);
				RemoveConflictingFlags(flairs, PostFlairs.CourtesyGood, PostFlairs.CourtesyBad);
			}
			return flairs;
		}

		public static PostFlairsParsed ParseFlairsEnum(this PostFlairs flairs) => flairs is 0 ? null : new
		(
			ParseBalancedFlags(flairs, PostFlairs.PerformanceGood, PostFlairs.PerformanceBad),
			ParseBalancedFlags(flairs, PostFlairs.TeamplayGood, PostFlairs.TeamplayBad),
			ParseBalancedFlags(flairs, PostFlairs.CourtesyGood, PostFlairs.CourtesyBad)
		);

		public static sbyte CountBalance(PostFlairsParsed flairs) => CountBalance(flairs?.Performance, flairs?.Teamplay, flairs?.Courtesy);
		public static sbyte CountBalance(params bool?[] flairs)
		{
			sbyte balance = 0;
			foreach (bool? flair in flairs)
			{
				if (flair is not null)
				{
					if (flair is true)
					{
						balance++;
					}
					else
					{
						balance--;
					}
				}
			}
			return balance;
		}

		private static PostFlairs RemoveConflictingFlags(PostFlairs flairs, PostFlairs flag1, PostFlairs flag2)
		{
			return flairs &= ((flairs & flag1) is not 0) ^ ((flairs & flag2) is not 0)
				? ~PostFlairs.Neutral 
				: ~(flag1 | flag2);
		}

		private static bool? ParseBalancedFlags(PostFlairs flairs, PostFlairs positive, PostFlairs negative)
		{
			if ((flairs & positive) is not 0)
			{
				return true;
			}
			else if ((flairs & negative) is not 0)
			{
				return false;
			}

			return null;
		}
	}
}
