using System;



namespace WowsKarma.Common.Models
{
	[Flags]
	public enum PostFlairs
	{
		Neutral = 0,

		PerformanceGood = 1,
		PerformanceBad = -1,

		TeamplayGood = 3,
		TeamplayBad = -3,

		CourtesyGood = 9,
		CourtesyBad = -9,
	}
}
