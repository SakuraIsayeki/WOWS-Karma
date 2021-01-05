using Microsoft.EntityFrameworkCore;
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

		public KarmaService(IDbContextFactory<ApiDbContext> contextFactory)
		{
			context = contextFactory.CreateDbContext();
		}

		public async Task UpdatePlayerKarmaAsync(uint playerId, PostFlairsParsed newFlairs, PostFlairsParsed oldFlairs, bool allowNegative)
		{
			Player player = await context.Players.FindAsync(playerId);

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

			await context.SaveChangesAsync();
		}

		public async Task UpdatePlayerRatingsAsync(uint playerId, PostFlairsParsed postFlairs, PostFlairsParsed oldFlairs)
		{
			Player player = await context.Players.FindAsync(playerId);

			player.PerformanceRating = UpdateRating(player.PerformanceRating, postFlairs?.Performance, oldFlairs?.Performance);
			player.TeamplayRating = UpdateRating(player.TeamplayRating, postFlairs?.Teamplay, oldFlairs?.Teamplay);
			player.CourtesyRating = UpdateRating(player.CourtesyRating, postFlairs?.Courtesy, oldFlairs?.Courtesy);

			await context.SaveChangesAsync();
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
