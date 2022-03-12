using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Threading;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Wargaming.Api.Client.Clients.Wows;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Clans;
using WowsKarma.Api.Data;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;
using ApiClanMember = Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Clans.ClanMember;
using ClanMember = WowsKarma.Api.Data.Models.ClanMember;

namespace WowsKarma.Api.Services;

public class ClanService
{
	public static TimeSpan ClanInfoUpdateSpan { get; } = TimeSpan.FromHours(4);
	public static TimeSpan ClanMemberUpdateSpan { get; } = TimeSpan.FromHours(1);
	
	private readonly ApiDbContext _context;
	private readonly WowsPublicApiClient _publicApi;
	private readonly WowsVortexApiClient _vortex;
	private readonly WowsClansApiClient _clansApi;

	public ClanService(ApiDbContext context, WowsPublicApiClient publicApi, WowsVortexApiClient vortex, WowsClansApiClient clansApi)
	{
		_context = context;
		_publicApi = publicApi;
		_vortex = vortex;
		_clansApi = clansApi;
	}

	public IQueryable<Clan> GetDbClans(bool includeMembers = false) => !includeMembers
		? _context.Clans
		: _context.Clans
			.Include(c => c.Members)
			.ThenInclude(m => m.Player);

	public async Task<IEnumerable<ClanListingDTO>> SearchClansAsync([MinLength(2)] string search, [Range(0, 500)] uint count = 20) =>
		(await _clansApi.SearchClansAsync(search, count))?.Clans.Select(
			x => new ClanListingDTO
			{
				Id = x.Id,
				Name = x.Name,
				Tag = x.Tag,
				LeagueColor = (uint)ColorTranslator.FromHtml(x.HexColor).ToArgb()
		});

	// FIXME: Caching is broken, values do not update.
	public async Task<Clan> GetClanAsync(uint clanId, bool includeMembers = false, CancellationToken ct = default)
	{
		Clan clan = await GetDbClans(includeMembers).FirstOrDefaultAsync(c => c.Id == clanId, ct);
		bool updateInfo = clan is null || ClanInfoUpdateNeeded(clan);
		bool updateMembers = includeMembers && (clan is null || ClanMembersUpdateNeeded(clan));
		
		if (updateInfo)
		{
			await UpdateClanInfo(clanId, clan, ct);
			await _context.SaveChangesAsync(ct);
		}

		if (updateMembers)
		{
			await UpdateClanMembersAsync(clan, ct);
			await _context.SaveChangesAsync(ct);
		}

		return clan;
	}

	internal async Task UpdateClanInfo(uint clanId, Clan clan, CancellationToken ct)
	{
		ClanInfo apiClan = (await _clansApi.FetchClanViewAsync(clanId, ct))?.Clan;
		clan = clan is null
			? apiClan?.Adapt<Clan>()
			: clan with
			{
				Tag = apiClan.Tag,
				Name = apiClan.Name,
				Description = apiClan.Description,
				LeagueColor = (uint)ColorTranslator.FromHtml(apiClan.Color).ToArgb()
			};

		clan.UpdatedAt = DateTime.UtcNow;
		_context.Clans.Upsert(clan);
	}

	private async Task UpdateClanMembersAsync(Clan clan, CancellationToken ct)
	{
		Dictionary<uint, ApiClanMember> members = (await _clansApi.FetchClanMembersAsync(clan.Id, ct: ct))!.Items!.ToDictionary(x => x.Id);
			
#pragma warning disable CA1841
		Dictionary<uint, Player> players = _context.Players.Where(p => members.Keys.Contains(p.Id)).ToDictionary(p => p.Id);
#pragma warning restore CA1841
			
		uint[] missing = members.Keys.Where(x => players.All(p => p.Key != x)).ToArray();
		uint[] outdated = players.Values.Where(PlayerService.UpdateNeeded).Select(p => p.Id).ToArray();

		foreach (uint id in missing)
		{
			Player dbPlayer = (await _vortex.FetchAccountAsync(id, ct)).ToDbModel();
			players.Add(id, dbPlayer);
			_context.Players.Add(dbPlayer);
		}
			
		foreach (uint id in outdated)
		{
			Player dbPlayer = Player.MapFromApi(players[id], await _context.Players.FindAsync(new object[] { id }, ct));
			_context.Attach(dbPlayer);
			players[id] = dbPlayer;
		}

		clan!.Members = new(members.Values.Select(x => new ClanMember
		{
			PlayerId = x.Id,
			Player = players[x.Id],
			ClanId = clan.Id,
			JoinedAt = DateOnly.FromDateTime(DateTime.UtcNow - TimeSpan.FromDays(x.DaysInClan)),
			Role = x.Role.Name
		}));
		
		_context.ClanMembers.UpsertRange(clan.Members);
		clan.MembersUpdatedAt = DateTime.UtcNow;
		_context.RemoveRange(_context.ClanMembers.Where(x => x.ClanId == clan.Id && !clan.Members.Select(y => y.PlayerId).Contains(x.PlayerId)));
	}

	internal static bool ClanInfoUpdateNeeded(Clan clan) => clan.UpdatedAt + ClanInfoUpdateSpan < DateTime.UtcNow;
	internal static bool ClanMembersUpdateNeeded(Clan clan) => clan.MembersUpdatedAt + ClanMemberUpdateSpan < DateTime.UtcNow;
}