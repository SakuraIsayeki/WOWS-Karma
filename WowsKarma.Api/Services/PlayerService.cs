using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Tags.Attributes;
using Nodsoft.Wargaming.Api.Client.Clients.Wows;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Public;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Vortex;
using WowsKarma.Api.Data;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Utilities;

namespace WowsKarma.Api.Services;

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

	/// <summary>
	/// Gets all players in the database.
	/// </summary>
	/// <returns>A non-tracked queryable collection of all players in the database.</returns>
	public IAsyncEnumerable<uint> ListPlayerIds() => _listPlayerIds(_context);
	private static readonly Func<ApiDbContext, IAsyncEnumerable<uint>> _listPlayerIds = EF.CompileAsyncQuery(
		(ApiDbContext context) => context.Players.AsNoTracking().Select(p => p.Id));
	
	
	[Tag("player", "update", "batch"), JobDisplayName("Perform API fetch on player batch")]
	public async Task<IEnumerable<Player>> GetPlayersAsync(IEnumerable<uint> ids, bool includeRelated = false, bool includeClanInfo = false, CancellationToken ct = default)
	{
		List<Player> players = [];
			
		foreach (uint id in ids.AsParallel().WithCancellation(ct))
		{
			players.Add(await GetPlayerAsync(id, includeRelated, includeClanInfo, ct));
		}

		return players;
	}

	/*
	* FIXME: (#38)
	*
	* Method no longer returns any tracked entity, resulting in dropped changes for EF Core
	* Do not use unless readonly.
	*/
	public async Task<Player> GetPlayerAsync(uint accountId, bool includeRelated = false, bool includeClanInfo = false, CancellationToken ct = default)
	{
		if (accountId is 0)
		{
			return null;
		}

		IQueryable<Player> dbPlayers = _context.Players;

		if (includeRelated)
		{
			dbPlayers = dbPlayers.Include(p => p.PlatformBans);
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
				player = player with { CreatedAt = DateTime.UtcNow };
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

		if (player.ClanMember is { ClanId: not 0 })
		{
			player.ClanMember = player.ClanMember with { Clan = await _clanService.GetClanAsync(player.ClanMember.ClanId, ct: ct) };
		}

		await _context.Players.Upsert(player).On(p => p.Id).RunAsync(ct);

		return player;
	}

	private static readonly Func<ApiDbContext, IEnumerable<uint>, IEnumerable<AccountFullKarmaDTO>> CompiledPlayersFullKarmaQuery =
		EF.CompileQuery(static (ApiDbContext db, IEnumerable<uint> accountIds) => db.Players.AsNoTracking()
			.Where(p => accountIds.Contains(p.Id))
			.Select(p => new AccountFullKarmaDTO(p.Id, p.GameKarma, p.SiteKarma, p.PerformanceRating, p.TeamplayRating, p.CourtesyRating)));
	
	public IEnumerable<AccountFullKarmaDTO> GetPlayersFullKarma(IEnumerable<uint> accountIds)
	{
		// Use the compiled query
		return CompiledPlayersFullKarmaQuery.Invoke(_context, accountIds);
	}
	
	/// <summary>
	/// Gets a karma readout for specified players.
	/// </summary>
	/// <param name="accountIds">The player account IDs to get karma for.</param>
	/// <returns>A async-enumerable collection of <see cref="AccountKarmaDTO"/> objects.</returns>
	public IEnumerable<AccountKarmaDTO> GetPlayersKarma(IEnumerable<uint> accountIds) => _getPlayersKarma(_context, accountIds);
	
	private static readonly Func<ApiDbContext, IEnumerable<uint>, IEnumerable<AccountKarmaDTO>> _getPlayersKarma = EF.CompileQuery(
		(ApiDbContext context, IEnumerable<uint> accountIds) => context.Players.AsNoTracking()
				.Where(p => accountIds.Contains(p.Id))
				.Select(p => new AccountKarmaDTO(p.Id, p.SiteKarma)));

	public async Task<AccountListingDTO[]> ListPlayersAsync(string search) 
		=> (await _wgApi.ListPlayersAsync(search))?.Data?.ToArray() is { Length: not 0 } result
			? result.Select(Conversions.ToDto).ToArray()
			: [];

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
				player.ClanMember = player.ClanMember! with { Role = apiResult!.Role };
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

	[JobDisplayName("Recalculate player metrics"), Tag("player", "maintenance", "metrics", "recalculation")]
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
	internal static bool IsOptOutOnCooldown(DateTimeOffset? lastChange) => lastChange is not null && lastChange + OptOutCooldownSpan > DateTimeOffset.UtcNow;

	private static void SetPlayerMetrics(Player player, int site, int performance, int teamplay, int courtesy)
	{
		player.SiteKarma = site;
		player.PerformanceRating = performance;
		player.TeamplayRating = teamplay;
		player.CourtesyRating = courtesy;
	}
}