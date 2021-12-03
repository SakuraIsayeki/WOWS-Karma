using Azure.Storage.Blobs;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.IO;
using System.Threading;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs.Replays;

namespace WowsKarma.Api.Services.Replays;

public class ReplaysIngestService
{
	public const string ReplayBlobContainer = "replays";
	public const int MaxReplaySize = 5242880;

	private readonly BlobServiceClient _serviceClient;
	private readonly BlobContainerClient _containerClient;
	private readonly ApiDbContext _context;
	private readonly ReplaysProcessService _processService;

	public ReplaysIngestService(IConfiguration configuration, ApiDbContext context, ReplaysProcessService processService)
	{
		string connectionString = configuration[$"API:{Startup.ApiRegion.ToRegionString()}:Azure:Storage:ConnectionString"];
		_serviceClient = new(connectionString);
		_containerClient = _serviceClient.GetBlobContainerClient(ReplayBlobContainer);
		_context = context;
		_processService = processService;
	}

	public Replay GetReplay(Guid id) => _context.Replays.Find(id);
	public async Task<ReplayDTO> GetReplayDTOAsync(Guid id)
	{
		Replay replay = _context.Replays.Find(id);

		return new()
		{
			Id = replay.Id,
			PostId = replay.PostId,
			ChatMessages = replay.ChatMessages.Adapt<IEnumerable<ReplayChatMessageDTO>>()
				.Select(m => m with { Username = replay.Players.FirstOrDefault(p => p.AccountId == m.PlayerId).Name }),
			Players = replay.Players.Adapt<IEnumerable<ReplayPlayerDTO>>(),
			DownloadUri = (await GenerateReplayDownloadLinkAsync(id)).ToString(),
		};
	}

	public async Task<Replay> IngestReplayAsync(Guid postId, IFormFile replayFile, CancellationToken ct)
	{
		// Over 5MB is too much for a WOWS Replay file.
		if (replayFile.Length is 0 or > MaxReplaySize)
		{
			throw new ArgumentOutOfRangeException(nameof(replayFile));
		}

		Post post = await _context.Posts.FindAsync(new object[] { postId }, cancellationToken: ct);

		Replay replay = _processService.ProcessReplay(new(), replayFile.OpenReadStream(), ct);

		// Past here, the replay is valid.

		if (post.ReplayId is Guid existingReplayId)
		{
			await RemoveReplayAsync(GetReplay(existingReplayId));
		}

		EntityEntry<Replay> entityEntry = _context.Replays.Add(replay with { PostId = postId });
		entityEntry.Entity.BlobName = $"{entityEntry.Entity.Id:N}-{replayFile.FileName}";

		// Set Post reverse nav to replay
		post.ReplayId = entityEntry.Entity.Id;

		await _containerClient.UploadBlobAsync(entityEntry.Entity.BlobName, replayFile.OpenReadStream(), ct);

		await _context.SaveChangesAsync(ct);
		return entityEntry.Entity;
	}

	public async Task<MemoryStream> FetchReplayFileAsync(Guid replayId, CancellationToken ct)
	{
		Replay replay = await _context.Replays.FindAsync(new object[] { replayId }, cancellationToken: ct)
			?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));

		BlobClient blobClient = _containerClient.GetBlobClient(replay.BlobName);

		using MemoryStream ms = new();
		await blobClient.DownloadToAsync(ms, ct);
		ms.Position = 0;

		return ms;
	}

	public async Task<Uri> GenerateReplayDownloadLinkAsync(Guid replayId)
	{
		Replay replay = await _context.Replays.FindAsync(replayId) ?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));
		return _containerClient.GetBlobClient(replay.BlobName).Uri;
	}

	public async Task RemoveReplayAsync(Replay replay)
	{
		await _containerClient.DeleteBlobAsync(replay.BlobName);

		_context.Replays.Remove(replay);
		await _context.SaveChangesAsync();
	}
}
