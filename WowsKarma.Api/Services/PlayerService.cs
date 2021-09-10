using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wargaming.WebAPI.Models.WorldOfWarships.Responses;
using Wargaming.WebAPI.Requests;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Utilities;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Services
{
	public class PlayerService
	{
		public static TimeSpan DataUpdateSpan { get; } = TimeSpan.FromHours(1);
		public static TimeSpan OptOutCooldownSpan { get; } = TimeSpan.FromDays(7);

		private readonly ApiDbContext context;
		private readonly WorldOfWarshipsHandler wgApi;
		private readonly VortexApiHandler vortex;


		public PlayerService(ApiDbContext context, WorldOfWarshipsHandler wgApi, VortexApiHandler vortex)
		{
			this.context = context;
			this.wgApi = wgApi;
			this.vortex = vortex;
		}


		/*
		 * FIXME: (#38)
		 *
		 * Method no longer returns any tracked entity, resulting in dropped changes for EF Core
		 * Do not use unless readonly.
		 */
		public async Task<Player> GetPlayerAsync(uint accountId)
		{
			if (accountId is 0)
			{
				return null;
			}

			Player player = await context.Players.FindAsync(accountId);

			try
			{
				if (player is null)
				{
					return await UpdatePlayerRecordAsync(accountId, true);
				}
				else if (UpdateNeeded(player))
				{
					return await UpdatePlayerRecordAsync(accountId, false);
				}
				else
				{
					return player;
				}
			}
			catch (ApplicationException)
			{
				return null;
			}
		}

		public IQueryable<AccountFullKarmaDTO> GetPlayersFullKarma(IEnumerable<uint> accountIds)
		{
			return from p in context.Players.AsNoTracking()
				   where accountIds.Contains(p.Id)
				   select new AccountFullKarmaDTO(p.Id, p.SiteKarma, p.PerformanceRating, p.TeamplayRating, p.CourtesyRating);
		}
		public IQueryable<AccountKarmaDTO> GetPlayersKarma(IEnumerable<uint> accountIds)
		{
			return from p in context.Players.AsNoTracking()
				   where accountIds.Contains(p.Id)
				   select new AccountKarmaDTO(p.Id, p.SiteKarma);
		}

		public async Task<IEnumerable<AccountListingDTO>> ListPlayersAsync(string search)
		{
			IEnumerable<AccountListing> result = await wgApi.ListPlayersAsync(search);

			return result?.Count() is not null or 0
				? result.Select(listing => listing.ToDTO())
				: null;
		}

		internal async Task<Player> UpdatePlayerRecordAsync(uint accountId, bool firstEntry)
		{
			Player player = (await vortex.FetchAccountAsync(accountId)).ToDbModel() ?? throw new ApplicationException("Account returned null.");

			if (firstEntry)
			{
				context.Players.Add(player);
			}
			else
			{
				player = Player.MapFromApi(await context.Players.FindAsync(accountId), player);
			}

			player.UpdatedAt = DateTime.UtcNow; // Forcing UpdatedAt refresh
			await context.SaveChangesAsync();
			return player;
		}

		public async Task UpdateProfileFlagsAsync(UserProfileFlagsDTO flags)
		{
			Player player = await GetPlayerAsync(flags.Id) ?? throw new ArgumentException("Player not found.");

			if (player.OptedOut != flags.OptedOut)
			{
				if (!IsOptOutOnCooldown(player.OptOutChanged))
				{
					player.OptedOut = flags.OptedOut;
					player.OptOutChanged = DateTime.UtcNow;
				}
				else
				{
					throw new CooldownException(nameof(OptOutCooldownSpan), player.OptOutChanged, DateTime.UtcNow);
				}
			}

			await context.SaveChangesAsync();
		}

		public async Task RecalculatePlayerMetrics(uint playerId, CancellationToken cancellationToken)
		{
			Player player = await context.Players.Include(p => p.PostsReceived).FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);

			if (cancellationToken.IsCancellationRequested || player is null)
			{
				return;
			}

			int oldSiteKarma = player.SiteKarma,
				oldPerformanceRating = player.PerformanceRating,
				oldTeamplayRating = player.TeamplayRating,
				oldCourtesyRating = player.CourtesyRating;

			try
			{
				SetPlayerMetrics(player, 0, 0, 0, 0);

				foreach (Post post in player.PostsReceived)
				{
					KarmaService.UpdatePlayerKarma(player, post.ParsedFlairs, null, post.NegativeKarmaAble);
					KarmaService.UpdatePlayerRatings(player, post.ParsedFlairs, null);
				}

				await context.SaveChangesAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				SetPlayerMetrics(player, oldSiteKarma, oldPerformanceRating, oldTeamplayRating, oldCourtesyRating);
				return;
			}
		}

		internal static bool UpdateNeeded(Player player) => player.UpdatedAt.Add(DataUpdateSpan) < DateTime.UtcNow;
		internal static bool IsOptOutOnCooldown(DateTime lastChange) => lastChange.Add(OptOutCooldownSpan) > DateTime.UtcNow;

		private static void SetPlayerMetrics(Player player, int site, int performance, int teamplay, int courtesy)
		{
			player.SiteKarma = site;
			player.PerformanceRating = performance;
			player.TeamplayRating = teamplay;
			player.CourtesyRating = courtesy;
		}
	}
}
