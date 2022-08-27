﻿using Azure.Storage.Blobs;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Security;
using System.Threading;
using Hangfire;
using Hangfire.Tags.Attributes;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs.Replays;

namespace WowsKarma.Api.Services.Replays;

public class ReplaysIngestService
{
	public const string ReplayBlobContainer = "replays";
	public const string SecurityBlobContainer = "rce-replays";
	public const int MaxReplaySize = 5242880;

	private readonly BlobServiceClient _serviceClient;
	private readonly BlobContainerClient _containerClient; // Container for standard replays
	private readonly BlobContainerClient _securityContainerClient; // Container for infected replays
	private readonly ILogger<ReplaysIngestService> _logger;
	private readonly ApiDbContext _context;
	private readonly ReplaysProcessService _processService;

	public ReplaysIngestService(ILogger<ReplaysIngestService> logger, IConfiguration configuration, ApiDbContext context, ReplaysProcessService processService)
	{
		string connectionString = configuration[$"API:{Startup.ApiRegion.ToRegionString()}:Azure:Storage:ConnectionString"];
		_serviceClient = new(connectionString);
		_containerClient = _serviceClient.GetBlobContainerClient(ReplayBlobContainer);
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

	public Replay GetReplay(Guid id) => _context.Replays.Find(id);
	
	public async Task<ReplayDTO> GetReplayDTOAsync(Guid id)
	{
		Replay replay = await _context.Replays.FindAsync(id);

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

		Replay replay = await _processService.ProcessReplayAsync(new Replay(), replayFile.OpenReadStream(), ct);

		// Past here, the replay is valid.

		if (post.ReplayId is { } existingReplayId)
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
		await _containerClient.UploadBlobAsync(entityEntry.Entity.BlobName, replayFile.OpenReadStream(), ct);
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
		_logger.LogInformation("Ingested RCE file {BlobName}. Link: {Uri}", blobName, _securityContainerClient.GetBlobClient(blobName).Uri);
	}
	
	public async Task<MemoryStream> FetchReplayFileAsync(Guid replayId, CancellationToken ct)
	{
		Replay replay = await _context.Replays.FindAsync(new object[] { replayId }, cancellationToken: ct)
			?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));

		BlobClient blobClient = _containerClient.GetBlobClient(replay.BlobName);

		await using MemoryStream ms = new();
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


	///  <summary>
	/// 		Reprocesses a replay file.
	/// 		This causes a replay to be downloaded from Azure storage and processed again.
	///  </summary>
	///  
	///  <param name="replay">
	/// 		The replay being reprocessed.
	/// 		Its ID and Blobname will be used to match the replay on Azure storage.
	///  </param>
	/// 
	///  <param name="ct"></param>
	public async Task ReprocessReplayAsync(Replay replay, CancellationToken ct)
	{
		await using MemoryStream ms = new();
		await _containerClient.GetBlobClient(replay.BlobName).DownloadToAsync(ms, ct);
		ms.Position = 0;

		try
		{
			await _processService.ProcessReplayAsync(replay, ms, ct);
		}
		// Catch any CVE-2022-31265 related exceptions and log them.
		catch (InvalidReplayException e) when (e.InnerException is SecurityException se && se.Data["exploit"] is "CVE-2022-31265")
		{
			_logger.LogWarning("CVE-2022-31265 exploit detected in replay {ReplayId}. Please delete both post and replay from the platform at once.", replay.Id);
		}
		catch (Exception e)
		{
			_logger.LogWarning(e, "Failed to reprocess replay {ReplayId}.", replay.Id);
		}
	}

	/// <summary>
	///		Reprocesses the replay file for the specified replay ID.
	///		This causes a replay to be downloaded from Azure storage and processed again.
	/// </summary>
	/// 
	/// <param name="replayId">The ID of the replay being reprocessed</param>
	/// <param name="ct"></param>
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
	///		Reprocesses all replays in the database submitted between the specified dates.
	///		This causes all corresponding replays to be downloaded from Azure storage and processed again.
	/// </summary>
	/// 
	/// <param name="start">Start of time range to select replays from</param>
	/// <param name="end">End of time range to select replays from</param>
	[JobDisplayName("Reprocess all replays within date range"), Tag("replay", "recalculation", "batch")]
	public async Task ReprocessAllReplaysAsync(DateTime? start, DateTime? end, CancellationToken ct)
	{
		_logger.LogWarning("Started reprocessing all replays between {Start:g} and {End:g}", start, end);

		List<Replay> replays = await _context.Posts.Include(p => p.Replay)
			.Where(r => r.Replay != null && r.CreatedAt >= start && r.CreatedAt <= end)
			.Select(r => new Replay
			{
				Id = r.Replay.Id,
				PostId = r.Replay.PostId,
				BlobName = r.Replay.BlobName
			}).ToListAsync(ct);
		
		_logger.LogWarning("Database readout complete. {Count} replays will be reprocessed.", replays.Count);

		// Process each replay in parallel.
		await Task.WhenAll(replays.Select(r => ReprocessReplayAsync(r, ct)));

		_logger.LogWarning("Finished file reprocessing of {Count} replays. Saving to database...", replays.Count);

		_context.UpdateRange(replays);
		await _context.SaveChangesAsync(ct);

		_logger.LogWarning("Replay Files reprocessing complete! Reprocessed {Count} replays total.", replays.Count);
	}
}
