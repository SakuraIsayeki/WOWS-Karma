using Azure.Storage.Blobs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security;
using Hangfire;
using Hangfire.Tags.Attributes;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs.Replays;

namespace WowsKarma.Api.Services.Replays;

public sealed class ReplaysIngestService
{
	public const string ReplayBlobContainer = "replays";
	public const string SecurityBlobContainer = "rce-replays";
	public const int MaxReplaySize = 5242880;

	private readonly BlobServiceClient _serviceClient;
	private readonly BlobContainerClient _replayContainerClient; // Container for standard replays
	private readonly BlobContainerClient _securityContainerClient; // Container for infected replays
	private readonly ILogger<ReplaysIngestService> _logger;
	private readonly ApiDbContext _context;
	private readonly ReplaysProcessService _processService;

	public ReplaysIngestService(ILogger<ReplaysIngestService> logger, IConfiguration configuration, ApiDbContext context, ReplaysProcessService processService)
	{
		string connectionString = configuration[$"API:{Startup.ApiRegion.ToRegionString()}:Azure:Storage:ConnectionString"]
			?? throw new InvalidOperationException("Missing API:Azure:Storage:ConnectionString in configuration.");
		
		_serviceClient = new(connectionString);
		_replayContainerClient = _serviceClient.GetBlobContainerClient(ReplayBlobContainer);
		_securityContainerClient = _serviceClient.GetBlobContainerClient(SecurityBlobContainer);
		_logger = logger;
		_context = context;
		_processService = processService;
	}

	/// <summary>
	/// List all replay IDs in the database.
	/// </summary>
	/// <returns>Async enumerable of replay IDs.</returns>
	public IAsyncEnumerable<Guid> ListReplaysAsync() => _listReplaysAsync(_context);
	private static readonly Func<ApiDbContext, IAsyncEnumerable<Guid>> _listReplaysAsync = EF.CompileAsyncQuery(
		(ApiDbContext context) => context.Replays.Select(r => r.Id));

	/// <summary>
	/// Gets a replay by its ID.
	/// </summary>
	/// <param name="id">The replay's ID.</param>
	/// <returns>The replay, or <see langword="null"/> if not found.</returns>
	public Replay? GetReplay(Guid id) => _context.Replays.Find(id);
	
	public async Task<ReplayDTO?> GetReplayDTOAsync(Guid id)
	{
		if (await _context.Replays.FindAsync(id) is not { } replay)
		{
			return null;
		}

		return new()
		{
			Id = replay.Id,
			PostId = replay.PostId,
			ChatMessages = replay.ChatMessages?.Adapt<IEnumerable<ReplayChatMessageDTO>>()
				.Select(m => m with { Username = replay.Players.FirstOrDefault(p => p.AccountId == m.PlayerId).Name }) ?? [],
			Players = replay.Players.Adapt<IEnumerable<ReplayPlayerDTO>>(),
			DownloadUri = $"{_replayContainerClient.Uri}/{replay.BlobName}",
			MinimapUri = replay.MinimapRendered ? $"{_serviceClient.Uri}{MinimapRenderingService.MinimapBlobContainer}/{replay.Id}.mp4" : null
		};
	}

	public async Task<Replay> IngestReplayAsync(Guid postId, IFormFile replayFile, CancellationToken ct)
	{
		// Over 5MB is too much for a WOWS Replay file.
		if (replayFile.Length is 0 or > MaxReplaySize)
		{
			throw new ArgumentOutOfRangeException(nameof(replayFile));
		}

		Post post = await _context.Posts.FindAsync([postId], cancellationToken: ct)
			?? throw new ArgumentException("No post was found for specified GUID.", nameof(postId));

		Replay replay = await _processService.ProcessReplayAsync(new Replay(), replayFile.OpenReadStream(), ct);

		// Past here, the replay is valid.

		if (post.ReplayId is { } existingReplayId)
		{
			await RemoveReplayAsync(GetReplay(existingReplayId) ?? throw new InvalidOperationException("Post has a replay ID, but no replay was found."));
		}

		EntityEntry<Replay> entityEntry = _context.Replays.Add(replay with { PostId = postId });
		entityEntry.Entity.BlobName = $"{entityEntry.Entity.Id:N}-{replayFile.FileName}";

		// Set Post reverse nav to replay
		post.ReplayId = entityEntry.Entity.Id;

		await _replayContainerClient.UploadBlobAsync(entityEntry.Entity.BlobName, replayFile.OpenReadStream(), ct);

		await _context.SaveChangesAsync(ct);

		return entityEntry.Entity;
	}
	
	public async Task<Replay> IngestReplayAsync(IFormFile replayFile, CancellationToken ct)
	{
		// Over 5MB is too much for a WOWS Replay file.
		if (replayFile.Length is 0 or > MaxReplaySize)
		{
			throw new ArgumentOutOfRangeException(nameof(replayFile));
		}

		Replay replay = await _processService.ProcessReplayAsync(new Replay(), replayFile.OpenReadStream(), ct);
		EntityEntry<Replay> entityEntry = _context.Replays.Add(replay);
		entityEntry.Entity.BlobName = $"{entityEntry.Entity.Id:N}-{replayFile.FileName}";
		await _replayContainerClient.UploadBlobAsync(entityEntry.Entity.BlobName, replayFile.OpenReadStream(), ct);
		await _context.SaveChangesAsync(ct);

		// Introspect the replay players list and add them to the database.
		uint[] players = replay.Players.Select(p => p.AccountId).ToArray();
		BackgroundJob.Enqueue<PlayerService>(x => x.GetPlayersAsync(players, false, false, ct));
		
		return replay;
	}

	internal async Task IngestRceFileAsync(IFormFile replayFile)
	{
		string blobName = $"{Guid.NewGuid():N}-{replayFile.FileName}";
		await _securityContainerClient.UploadBlobAsync(blobName, replayFile.OpenReadStream());
		_logger.LogInformation("Ingested RCE file {blobName}. Link: {uri}", blobName, _securityContainerClient.GetBlobClient(blobName).Uri);
	}
	
	public async Task<MemoryStream> FetchReplayFileAsync(Guid replayId, CancellationToken ct)
	{
		Replay replay = await _context.Replays.FindAsync(new object[] { replayId }, cancellationToken: ct)
			?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));

		BlobClient blobClient = _replayContainerClient.GetBlobClient(replay.BlobName);

		MemoryStream ms = new();
		await blobClient.DownloadToAsync(ms, ct);
		ms.Position = 0;

		return ms;
	}

	public async Task<Uri> GenerateReplayDownloadLinkAsync(Guid replayId)
	{
		Replay replay = await _context.Replays.FindAsync(replayId) ?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));
		return _replayContainerClient.GetBlobClient(replay.BlobName).Uri;
	}

	public async Task RemoveReplayAsync(Replay replay)
	{
		await _replayContainerClient.DeleteBlobAsync(replay.BlobName);

		_context.Replays.Remove(replay);
		await _context.SaveChangesAsync();
	}


	/// <summary>
	/// Reprocesses a replay file.
	/// This causes a replay to be downloaded from Azure storage and processed again.
	/// </summary>
	///  
	/// <param name="replay">
	/// The replay being reprocessed.
	/// Its ID and Blobname will be used to match the replay on Azure storage.
	/// </param>
	/// <param name="ct">Cancellation token.</param>
	public async Task<Replay?> ReprocessReplayAsync(Replay replay, CancellationToken ct)
	{
		await using MemoryStream ms = new();
		await _replayContainerClient.GetBlobClient(replay.BlobName).DownloadToAsync(ms, ct);
		ms.Position = 0;

		try
		{
			return await _processService.ProcessReplayAsync(replay, ms, ct);
		}
		// Catch any CVE-2022-31265 related exceptions and log them.
		catch (InvalidReplayException e) when (e.InnerException is SecurityException se && se.Data["exploit"] is "CVE-2022-31265")
		{
			_logger.LogWarning("CVE-2022-31265 exploit detected in replay {replayId}. Please delete both post and replay from the platform at once.", replay.Id);
		}
		catch (Exception e)
		{
			_logger.LogWarning(e, "Failed to reprocess replay {replayId}.", replay.Id);
		}

		return null;
	}

	/// <summary>
	///	Reprocesses the replay file for the specified replay ID.
	///	This causes a replay to be downloaded from Azure storage and processed again.
	/// </summary>
	/// 
	/// <param name="replayId">The ID of the replay being reprocessed</param>
	/// <param name="ct">Cancellation token.</param>
	/// <exception cref="ArgumentException">Thrown when no replay was found.</exception>
	[JobDisplayName("Reprocess single replay"), Tag("replay", "recalculation", "single")]
	public async Task ReprocessReplayAsync(Guid replayId, CancellationToken ct)
	{
		Replay replay = await _context.Replays.FindAsync(new object[] { replayId }, cancellationToken: ct) 
			?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));
		
		await ReprocessReplayAsync(replay, ct);
		await _context.SaveChangesAsync(ct);
	}

	/// <summary>
	/// Reprocesses all replays in the database submitted between the specified dates.
	/// This causes all corresponding replays to be downloaded from Azure storage and processed again.
	/// </summary>
	/// <param name="start">Start of time range to select replays from</param>
	/// <param name="end">End of time range to select replays from</param>
	/// <param name="ct">Cancellation token</param>
	[JobDisplayName("Reprocess all replays within date range"), Tag("replay", "recalculation", "batch")]
	public async Task ReprocessAllReplaysAsync(DateTime? start, DateTime? end, CancellationToken ct)
	{
		_logger.LogWarning("Started reprocessing all replays between {start:g} and {end:g}", start, end);

		var replayStubs = await _context.Posts.Include(static p => p.Replay)
			.Where(r => r.Replay != null && r.CreatedAt >= start && r.CreatedAt <= end)
			.Select(static p => new Replay
				{
					Id = p.Replay.Id,
					BlobName = p.Replay.BlobName,
					PostId = p.Id
				})
			.ToArrayAsync(ct);
		
		_logger.LogWarning("Database readout complete. {count} replays will be reprocessed.", replayStubs.Length);

		List<Replay> replays = [];

		foreach (Replay replay in replayStubs)
		{
			if (await ReprocessReplayAsync(replay, ct) is { } r)
			{
				replays.Add(r);
			}
		}

		_logger.LogWarning("Finished file reprocessing of {count} replays. Saving to database...", replayStubs.Length);

		_context.UpdateRange(replays);
		await _context.SaveChangesAsync(ct);

		_logger.LogWarning("Replay Files reprocessing complete! Reprocessed {count} replays total.", replayStubs.Length);
	}
}
