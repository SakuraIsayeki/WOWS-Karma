using Microsoft.EntityFrameworkCore;
using System.Threading;
using Nodsoft.Wargaming.Api.Client.Clients.Wows;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Public;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Vortex;
using WowsKarma.Api.Data;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;

namespace WowsKarma.Api.Services
{
	public class PlayerService
	{
		public static TimeSpan DataUpdateSpan { get; } = TimeSpan.FromHours(1);
		public static TimeSpan OptOutCooldownSpan { get; } = TimeSpan.FromDays(7);

		private readonly ApiDbContext _context;
		private readonly WowsPublicApiClient _wgApi;
		private readonly WowsVortexApiClient _vortex;
		private readonly ClanService _clanService;


		public PlayerService(ApiDbContext context, WowsPublicApiClient wgApi, WowsVortexApiClient vortex, ClanService clanService)
		{
			_context = context;
			_wgApi = wgApi;
			_vortex = vortex;
			_clanService = clanService;
		}


		/*
		 * FIXME: (#38)
		 *
		 * Method no longer returns any tracked entity, resulting in dropped changes for EF Core
		 * Do not use unless readonly.
		 */
		public async Task<Player> GetPlayerAsync(uint accountId, bool includeRelated = false, bool includeClanInfo = true, CancellationToken ct = default)
		{
			if (accountId is 0)
			{
				return null;
			}

			IQueryable<Player> dbPlayers = _context.Players;

			if (includeRelated)
			{
				_context.Players
					.Include(p => p.PlatformBans);
			}

			if (includeClanInfo)
			{
				dbPlayers = dbPlayers
					.Include(p => p.ClanMember)
						.ThenInclude(cm => cm.Clan);
			}


			Player player = await dbPlayers.FirstOrDefaultAsync(p => p.Id == accountId, ct);
			bool updated = false;
			bool insert = player is null;

			if (insert || UpdateNeeded(player))
			{
				player = await UpdatePlayerRecordAsync(player, accountId);
				updated = true;

				if (insert)
				{
					await _context.SaveChangesAsync(ct);
				}
			}

			if (includeClanInfo)
			{
				player = await UpdatePlayerClanStatusAsync(player, ct);
				updated = true;
			}

			if (updated)
			{
				player.UpdatedAt = DateTime.UtcNow;
			}

			await _context.SaveChangesAsync(ct);

			if (player.ClanMember is { ClanId: not 0 })
			{
				player.ClanMember = player.ClanMember with { Clan = await _clanService.GetClanAsync(player.ClanMember.ClanId, ct: ct) };
			}
			
			return player;
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

		internal async Task<Player> UpdatePlayerClanStatusAsync(Player player, CancellationToken ct = default)
		{
			// Bypass if individual player was recently updated, or if clan members were recently updated and supercedes player update.
			if (player?.ClanMember?.Clan?.MembersUpdatedAt is { } lastClanUpdate 
				&& DateTime.UtcNow > lastClanUpdate + ClanService.ClanMemberUpdateSpan
				&& lastClanUpdate > player!.UpdatedAt
				|| DateTime.UtcNow < player!.UpdatedAt + ClanService.ClanMemberUpdateSpan)
			{
				return player;
			}

			VortexAccountClanInfo apiResult = await _vortex.FetchAccountClanAsync(player.Id, ct);
			Clan clan = null;
			
			if (apiResult?.ClanId is not null)
			{
				clan = await _context.Clans.FirstOrDefaultAsync(c => c.Id == apiResult.ClanId, ct);

				if (clan is null || ClanService.ClanInfoUpdateNeeded(clan))
				{
					await _clanService.UpdateClanInfoAsync(_context, apiResult.ClanId.Value, clan, ct);
				}
			}

			if (player.ClanMember?.ClanId != apiResult?.ClanId)
			{
				if (player.ClanMember is not null)
				{
					_context.ClanMembers.Remove(player.ClanMember);
				}
				
				player.ClanMember = apiResult.ClanId is null
					? null
					: new()
					{
						PlayerId = player.Id,
						ClanId = apiResult.ClanId.Value,
						Clan = clan,
						JoinedAt = DateOnly.FromDateTime(apiResult.JoinedAt.Value),
						Role = apiResult.Role
					};
			}
			else
			{
				if (player.ClanMember is not null)
				{
					player.ClanMember = player.ClanMember! with
					{
						Role = apiResult!.Role
					};
				}
			}
			
			return player;
		}

		internal async Task<Player> UpdatePlayerRecordAsync(Player player, uint? accountId = null)
		{
			Player apiPlayer = (await _vortex.FetchAccountAsync(player?.Id ?? accountId ?? 0)).ToDbModel() ?? throw new ApplicationException("Account returned null.");

			player = player is null 
				? _context.Players.Add(apiPlayer).Entity 
				: Player.MapFromApi(player, apiPlayer);
			
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

		internal static bool UpdateNeeded(Player player) => player.UpdatedAt + DataUpdateSpan < DateTime.UtcNow;
		internal static bool IsOptOutOnCooldown(DateTime lastChange) => lastChange + OptOutCooldownSpan > DateTime.UtcNow;

		private static void SetPlayerMetrics(Player player, int site, int performance, int teamplay, int courtesy)
		{
			player.SiteKarma = site;
			player.PerformanceRating = performance;
			player.TeamplayRating = teamplay;
			player.CourtesyRating = courtesy;
		}
	}
}
