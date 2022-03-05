namespace WowsKarma.Api.Services
{
	public class KarmaService
	{
		public KarmaService() { }

		public static void UpdatePlayerKarma(Player player, PostFlairsParsed newFlairs, PostFlairsParsed oldFlairs, bool allowNegative)
		{
			sbyte? newKarmaBalance = newFlairs is null ? null : PostFlairsUtils.CountBalance(newFlairs);
			sbyte? oldKarmaBalance = oldFlairs is null ? null : PostFlairsUtils.CountBalance(oldFlairs);

			// Revert Old Karma change
			if (oldKarmaBalance is not null and not 0)
			{
				if (oldKarmaBalance > 0)
				{
					player.SiteKarma--;
				}
				else
				{
					player.SiteKarma++;
				}
			}

			// Apply new Karma
			if (newKarmaBalance is not null and not 0)
			{
				if (newKarmaBalance > 0)
				{
					player.SiteKarma++;
				}
				else if (newKarmaBalance < 0 && allowNegative)
				{
					player.SiteKarma--;
				}
			}
		}

		public static void UpdatePlayerRatings(Player player, PostFlairsParsed postFlairs, PostFlairsParsed oldFlairs)
		{
			player.PerformanceRating = UpdateRating(player.PerformanceRating, postFlairs?.Performance, oldFlairs?.Performance);
			player.TeamplayRating = UpdateRating(player.TeamplayRating, postFlairs?.Teamplay, oldFlairs?.Teamplay);
			player.CourtesyRating = UpdateRating(player.CourtesyRating, postFlairs?.Courtesy, oldFlairs?.Courtesy);
		}

		private static int UpdateRating(int rating, bool? newFlair, bool? oldFlair)
		{
			// Revert old flair
			if (oldFlair is true)
			{
				rating--;
			}
			else if (oldFlair is false)
			{
				rating++;
			}


			// Apply new flair
			if (newFlair is true)
			{
				rating++;
			}
			else if (newFlair is false)
			{
				rating--;
			}

			return rating;
		}
	}
}
