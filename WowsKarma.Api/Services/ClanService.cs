using System.Drawing;
using System.Threading;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Wargaming.Api.Client.Clients.Wows;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Clans;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Vortex;
using WowsKarma.Api.Data;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;
using ApiClanMember = Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Clans.ClanMember;
using ClanMember = WowsKarma.Api.Data.Models.ClanMember;

namespace WowsKarma.Api.Services;

public class ClanService
{
	public static Duration DataUpdateSpan { get; } = Duration.FromHours(1);
	
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
	
	public async Task<Clan> GetClanAsync(uint clanId, bool includeMembers = false, CancellationToken ct = default)
	{
		Clan dbClan = await GetDbClans(includeMembers).FirstOrDefaultAsync(c => c.Id == clanId, ct);

		if (dbClan is null)
		{
			dbClan = await UpdateClanAsync(clanId, true, includeMembers, ct);
		}
		else if (UpdateNeeded(dbClan))
		{
			dbClan = await UpdateClanAsync(clanId, false, includeMembers, ct);
		}

		return dbClan;
	}

	internal async Task<Clan> UpdateClanAsync(uint clanId, bool firstEntry, bool updateMembers = true, CancellationToken ct = default)
	{
		ClanInfo apiClan = (await _clansApi.FetchClanViewAsync(clanId, ct))?.Clan;
		
		Clan dbClan = firstEntry
			? apiClan?.Adapt<Clan>()
			: await GetDbClans(updateMembers).FirstAsync(c => c.Id == clanId, ct) with
			{
				Tag = apiClan.Tag,
				Name = apiClan.Name,
				Description = apiClan.Description,
				LeagueColor = (uint)ColorTranslator.FromHtml(apiClan.Color).ToArgb()
			};

		if (updateMembers)
		{
			Dictionary<uint, ApiClanMember> members = (await _clansApi.FetchClanMembersAsync(clanId, ct: ct))!.Items!.ToDictionary(x => x.Id);
			
			#pragma warning disable CA1841
			Dictionary<uint, Player> players = _context.Players.Where(p => members.Keys.Contains(p.Id)).ToDictionary(p => p.Id);
			#pragma warning restore CA1841
			
			uint[] missing = members.Keys.Where(x => players.All(p => p.Key != x)).ToArray();
			uint[] outdated = players.Values.Where(PlayerService.UpdateNeeded).Select(p => p.Id).ToArray();

			Dictionary<uint, Player> missingResolved = new();


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

			dbClan!.Members = new(members.Values.Select(x => new ClanMember
			{
				PlayerId = x.Id,
				Player = players[x.Id],
				ClanId = clanId,
				JoinedAt = Time.Now.InUtc().Date.Minus(Period.FromDays(Convert.ToInt32(x.DaysInClan))),
				Role = x.Role.Name
			}));

			
		}

		if (firstEntry)
		{
			_context.Clans.Add(dbClan);
		}
		
		_context.AttachRange(dbClan.Members.Where(m => _context.ClanMembers.Contains(m)));
		await _context.AddRangeAsync(dbClan.Members.Except(_context.ClanMembers.Where(m => dbClan.Members.Contains(m))), ct);

		await _context.SaveChangesAsync(ct);
		return dbClan;
	}
	
	internal static bool UpdateNeeded(Clan clan) => clan.UpdatedAt + DataUpdateSpan < Time.Now;
}