using Mapster;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nodsoft.WowsReplaysUnpack.ExtendedData;
using Nodsoft.WowsReplaysUnpack.ExtendedData.Models;
using Nodsoft.WowsReplaysUnpack.Services;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Infrastructure.Exceptions;
using ReplayPlayer = WowsKarma.Api.Data.Models.Replays.ReplayPlayer;
using ReplayPlayerRaw = Nodsoft.WowsReplaysUnpack.ExtendedData.Models.ReplayPlayer;


namespace WowsKarma.Api.Services.Replays;

public sealed class ReplaysProcessService
{
	public static JsonSerializerOptions SerializerOptions { get; } = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
		UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
		IgnoreReadOnlyProperties = true
	};

	private readonly IReplayUnpackerService<ExtendedDataReplay> _replayUnpacker;
	private readonly ApiDbContext _context;

	public ReplaysProcessService(ReplayUnpackerFactory replayUnpacker, ApiDbContext context)
	{
		_replayUnpacker = replayUnpacker.GetExtendedDataUnpacker();
		_context = context;
	}

	public async Task<Replay> ProcessReplayAsync(Guid replayId, Stream replayStream, CancellationToken ct)
	{
		using Replay replay = await _context.Replays.FindAsync([replayId], cancellationToken: ct) 
			?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));

		await ProcessReplayAsync(replay, replayStream, ct);

		await _context.SaveChangesAsync(ct);
		return replay;
	}

	public Task<Replay> ProcessReplayAsync(Replay replay, Stream replayStream, CancellationToken ct)
	{
		try
		{
			ct.ThrowIfCancellationRequested();

			ExtendedDataReplay replayRaw = _replayUnpacker.Unpack(replayStream);

			replay.ArenaInfo = JsonSerializer.SerializeToDocument(replayRaw.ArenaInfo);
			replay.Players = ProcessReplayPlayers(replayRaw.ReplayPlayers);
			replay.ChatMessages = replayRaw.ChatMessages.Select(m => new ReplayChatMessage 
			{
				EntityId = m.EntityId,
				MessageContent = m.MessageContent,
				MessageGroup = m.MessageGroup switch
				{
					ReplayMessageGroup.All => "battle_common",
					ReplayMessageGroup.Team => "battle_team",
					_ => "battle_prebattle"
				},
				
				// Past 0.11.4, old Player AvatarId was moved to Id
				PlayerId = replay.Players.FirstOrDefault(p => p.Id == m.EntityId).AccountId is not 0 and var playerId ? playerId 
					: replay.Players.FirstOrDefault(p => p.AvatarId == m.EntityId).AccountId
			});

			return Task.FromResult(replay);
		}
		catch (OperationCanceledException e)
		{
			return Task.FromCanceled<Replay>(e.CancellationToken);
		}
		catch (Exception e)
		{
			throw new InvalidReplayException(e);
		}
	}

	public static IEnumerable<ReplayPlayer> ProcessReplayPlayers(IEnumerable<ReplayPlayerRaw> players) => players.Adapt<IEnumerable<ReplayPlayer>>();
}