using System.IO;
using System.Threading;
using Azure.Storage.Blobs;
using Hangfire;
using Hangfire.Tags.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Minimap.Client;
using WowsKarma.Api.Services.Replays;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Minimap;

/// <summary>
/// Provides minimap rendering for existing post replays.
/// </summary>
public sealed class MinimapRenderingService
{
	public const string MinimapBlobContainer = "minimaps";
	
	private readonly MinimapApiClient _client;
	private readonly ReplaysIngestService _replaysIngestService;
	private readonly ApiDbContext _context;
	private readonly ILogger<MinimapRenderingService> _logger;
	private readonly BlobContainerClient _containerClient;

	public MinimapRenderingService(
		MinimapApiClient client, 
		ReplaysIngestService replaysIngestService, 
		ApiDbContext context, 
		ILogger<MinimapRenderingService> logger,
		IConfiguration configuration
	) {
		_client = client;
		_replaysIngestService = replaysIngestService;
		_context = context;
		_logger = logger;
		
		string connectionString = configuration[$"API:{Startup.ApiRegion.ToRegionString()}:Azure:Storage:ConnectionString"];
		BlobServiceClient serviceClient = new(connectionString);
		_containerClient = serviceClient.GetBlobContainerClient(MinimapBlobContainer);
	}

	/// <summary>
	/// Renders the minimap for the specified post's replay.
	/// </summary>
	/// <param name="postId">The ID of the post to render the minimap for.</param>
	/// <param name="targetedPlayerId">(optional) The ID of a player to highlight/target on the minimap.</param>
	/// <param name="ct">The cancellation token.</param>
	[Tag("minimap", "replay", "render"), JobDisplayName("Render replay minimap for post {0}")]
	public async Task RenderPostReplayMinimapAsync(Guid postId, uint? targetedPlayerId = null, CancellationToken ct = default)
	{
		_logger.LogDebug("Rendering minimap for post {postId}.", postId);
		
		Post post = await _context.Posts.Include(static r => r.Replay).FirstOrDefaultAsync(p => p.Id == postId, cancellationToken: ct) 
			?? throw new ArgumentException($"Post with ID {postId} does not exist.", nameof(postId));

		if (post.Replay is null or { MinimapRendered: true })
		{
			_logger.LogInformation("Skipping minimap rendering for post {postId}.", postId);
            return;
		}
		
		await using MemoryStream ms = await _replaysIngestService.FetchReplayFileAsync(post.Replay.Id, ct);
		
		_logger.LogDebug("Rendering minimap for post {postId} from replay {replayId}.", postId, post.Replay.Id);
		ms.Position = 0;
		
		byte[] response = await _client.RenderReplayMinimapAsync(ms.ToArray(), post.Replay.Id.ToString(), targetedPlayerId, ct);
        _logger.LogDebug("Minimap rendered for post {postId} from replay {replayId}.", postId, post.Replay.Id);
		
		await UploadReplayMinimapAsync(post.Replay.Id, response, ct);
        
		post.Replay.MinimapRendered = true;
		await _context.SaveChangesAsync(ct);
	}
	
	/// <summary>
	/// Uploads the rendered minimap for the specified post's replay to Azure storage.
	/// </summary>
	/// <param name="replayId">The ID of the replay to upload the minimap for.</param>
	/// <param name="content">The minimap video as a blob.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <exception cref="ArgumentException">Thrown when no post was found.</exception>
	public async ValueTask UploadReplayMinimapAsync(Guid replayId, byte[] content, CancellationToken ct = default)
	{
		_logger.LogDebug("Uploading minimap for replay {replayId}.", replayId);
		
		Post post = await _context.Posts.Include(static r => r.Replay).FirstOrDefaultAsync(p => p.Replay.Id == replayId, ct) 
			?? throw new ArgumentException($"Post with replay ID {replayId} does not exist.", nameof(replayId));

		if (post.Replay is null or { MinimapRendered: true })
		{
			_logger.LogInformation("Skipping minimap upload for replay {replayId}.", replayId);
			return;
		}
		
		_logger.LogDebug("Uploading minimap for replay {replayId} to Azure storage.", replayId);
		
		await using MemoryStream ms = new(content);
		ms.Position = 0;
		
		await _containerClient.UploadBlobAsync(post.Replay.Id.ToString(), ms, ct);
	}

	public async ValueTask ReprocessAllMinimapsAsync(DateTime? start, DateTime? end, CancellationToken ct)
	{
		_logger.LogWarning("Started reprocessing all replay minimaps between {start:g} and {end:g}", start, end);
		
		var replays = 
			from replay in _context.Replays.Include(static r => r.Post)
			where replay.Post.CreatedAt >= start && replay.Post.CreatedAt <= end
			select replay;

		await foreach (Replay replay in replays.AsAsyncEnumerable().WithCancellation(ct))
		{
			_logger.LogDebug("Reprocessing replay minimap {replayId}", replay.Id);
			await RenderPostReplayMinimapAsync(replay.Id, replay.Post.PlayerId, ct);
		}
	}
	
	public async ValueTask<Uri> GenerateMinimapUriAsync(Guid replayId)
	{
		Replay replay = await _context.Replays.FindAsync(replayId) ?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));
		return _containerClient.GetBlobClient(replay.BlobName).Uri;
	}
}