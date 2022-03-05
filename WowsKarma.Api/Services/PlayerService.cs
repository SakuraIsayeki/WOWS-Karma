using Microsoft.EntityFrameworkCore;
using System.Threading;
using Nodsoft.Wargaming.Api.Client.Clients.Wows;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Public;
using WowsKarma.Api.Data;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Utilities;

namespace WowsKarma.Api.Services
{
	public class PlayerService
	{
		public static TimeSpan DataUpdateSpan { get; } = TimeSpan.FromHours(1);
		public static TimeSpan OptOutCooldownSpan { get; } = TimeSpan.FromDays(7);

		private readonly ApiDbContext _context;
		private readonly WowsPublicApiClient _wgApi;
		private readonly WowsVortexApiClient _vortex;


		public PlayerService(ApiDbContext context, WowsPublicApiClient wgApi, WowsVortexApiClient vortex)
		{
			_context = context;
			_wgApi = wgApi;
			_vortex = vortex;
		}


		/*
		 * FIXME: (#38)
		 *
		 * Method no longer returns any tracked entity, resulting in dropped changes for EF Core
		 * Do not use unless readonly.
		 */
		public async Task<Player> GetPlayerAsync(uint accountId, bool includeRelated = false)
		{
			if (accountId is 0)
			{
				return null;
			}

			IQueryable<Player> dbPlayers = !includeRelated
				? _context.Players
				: _context.Players
					.Include(p => p.PlatformBans);



			Player player = await dbPlayers.FirstOrDefaultAsync(p => p.Id == accountId);

			try
			{
				if (player is null)
				{
					return await UpdatePlayerRecordAsync(accountId, true);
				}

				if (UpdateNeeded(player))
				{
					return await UpdatePlayerRecordAsync(accountId, false);
				}

				return player;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public IQueryable<AccountFullKarmaDTO> GetPlayersFullKarma(IEnumerable<uint> accountIds)
		{
			return from p in _context.Players.AsNoTracking()
				   where accountIds.Contains(p.Id)
				   select new AccountFullKarmaDTO(p.Id, p.SiteKarma, p.PerformanceRating, p.TeamplayRating, p.CourtesyRating);
		}
		public IQueryable<AccountKarmaDTO> GetPlayersKarma(IEnumerable<uint> accountIds)
		{
			return from p in _context.Players.AsNoTracking()
				   where accountIds.Contains(p.Id)
				   select new AccountKarmaDTO(p.Id, p.SiteKarma);
		}

		public async Task<IEnumerable<AccountListingDTO>> ListPlayersAsync(string search)
		{
			AccountListing[] result = (await _wgApi.ListPlayersAsync(search))?.Data?.ToArray();

			return result is { Length: > 0 }
				? result.Select(listing => listing.ToDTO())
				: null;
		}

		internal async Task<Player> UpdatePlayerRecordAsync(uint accountId, bool firstEntry)
		{
			Player player = (await _vortex.FetchAccountAsync(accountId)).ToDbModel() ?? throw new ApplicationException("Account returned null.");

			if (firstEntry)
			{
				_context.Players.Add(player);
			}
			else
			{
				player = Player.MapFromApi(await _context.Players.FindAsync(accountId), player);
			}

			player.UpdatedAt = DateTime.UtcNow; // Forcing UpdatedAt refresh
			await _context.SaveChangesAsync();
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

			await _context.SaveChangesAsync();
		}

		public async Task RecalculatePlayerMetrics(uint playerId, CancellationToken ct)
		{
			Player player = await _context.Players.Include(p => p.PostsReceived).FirstOrDefaultAsync(p => p.Id == playerId, ct);

			if (ct.IsCancellationRequested || player is null)
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

				await _context.SaveChangesAsync(ct);
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
