using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Common.Models;

namespace WowsKarma.Api.Services
{
	public class KarmaService
	{
		private readonly ApiDbContext context;

		public KarmaService()
		{
			PostService.PostAdded += OnPostAdded;
			PostService.PostUpdated += OnPostUpdated;
			PostService.PostDeleted += OnPostDeleted;
		}

		public void OnPostAdded(object _, PostEventArgs e)
		{
			UpdatePlayerKarma(e.Post.Player, e.Post.ParsedFlairs, null, e.Post.NegativeKarmaAble);
			UpdatePlayerRatings(e.Post.Player, e.Post.ParsedFlairs, null);
		}

		public void OnPostUpdated(object _, PostUpdatedEventArgs e)
		{
			UpdatePlayerKarma(e.Post.Player, e.Post.ParsedFlairs, e.PreviousFlairs, e.Post.NegativeKarmaAble);
			UpdatePlayerRatings(e.Post.Player, e.Post.ParsedFlairs, e.PreviousFlairs);
		}

		public void OnPostDeleted(object _, PostEventArgs e)
		{
			UpdatePlayerKarma(e.Post.Player, null, e.Post.ParsedFlairs, e.Post.NegativeKarmaAble);
			UpdatePlayerRatings(e.Post.Player, null, e.Post.ParsedFlairs);
		}


		public static void UpdatePlayerKarma(Player player, PostFlairsParsed newFlairs, PostFlairsParsed oldFlairs, bool allowNegative)
		{
			byte? newKarmaBalance = newFlairs is null ? null : PostFlairsUtils.CountBalance(newFlairs);
			byte? oldKarmaBalance = oldFlairs is null ? null : PostFlairsUtils.CountBalance(oldFlairs);

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
			player.PerformanceRating = UpdateRating(player.PerformanceRating, postFlairs.Performance, oldFlairs.Performance);
			player.TeamplayRating = UpdateRating(player.TeamplayRating, postFlairs.Teamplay, oldFlairs.Teamplay);
			player.CourtesyRating = UpdateRating(player.CourtesyRating, postFlairs.Courtesy, oldFlairs.Courtesy);
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
			if (oldFlair is true)
			{
				rating++;
			}
			else if (oldFlair is false)
			{
				rating--;
			}

			return rating;
		}
	}
}
