using Mapster;
using Nodsoft.WowsReplaysUnpack;
using Nodsoft.WowsReplaysUnpack.Data;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;

using ReplayPlayer = WowsKarma.Api.Data.Models.Replays.ReplayPlayer;
using ReplayPlayerRaw = Nodsoft.WowsReplaysUnpack.Data.ReplayPlayer;

namespace WowsKarma.Api.Services.Replays;

public class ReplaysProcessService
{
	public static JsonSerializerOptions SerializerOptions { get; } = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
		UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
		IgnoreReadOnlyProperties = true
	};



	private readonly ReplayUnpacker _replayUnpacker;
	private readonly ReplaysIngestService _ingestService;
	private readonly ApiDbContext _context;

	public ReplaysProcessService(ReplayUnpacker replayUnpacker, ReplaysIngestService ingestService, ApiDbContext context)
	{
		_replayUnpacker = replayUnpacker;
		_ingestService = ingestService;
		_context = context;
	}

	public async Task<Replay> ProcessReplayAsync(Guid replayId, Stream replayStream, CancellationToken ct)
	{
		Replay replay = await _context.Replays.FindAsync(new object[] { replayId }, cancellationToken: ct)
			?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));

		replayStream ??= await _ingestService.FetchReplayFileAsync(replayId, ct);
		ReplayRaw replayRaw = _replayUnpacker.UnpackReplay(replayStream);
		replay.ArenaInfo = JsonSerializer.Deserialize<ReplayArenaInfo>(replayRaw.ArenaInfoJson, SerializerOptions);
		replay.Players = ProcessReplayPlayers(replayRaw.ReplayPlayers);
		replay.ChatMessages = replayRaw.ChatMessages.Select(m => new ReplayChatMessage()
		{
			EntityId = m.EntityId,
			MessageContent = m.MessageContent,
			MessageGroup = m.MessageGroup,
			PlayerId = replay.Players.FirstOrDefault(p => p.AvatarId == m.EntityId).AccountId
		});

		await _context.SaveChangesAsync(ct);
		return replay;
	}

	public IEnumerable<ReplayPlayer> ProcessReplayPlayers(IEnumerable<ReplayPlayerRaw> players) => players.Adapt<IEnumerable<ReplayPlayer>>();
}
