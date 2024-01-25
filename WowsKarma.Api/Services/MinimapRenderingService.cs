using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Hangfire;
using Hangfire.Tags.Attributes;
using Microsoft.EntityFrameworkCore;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Minimap.Client;
using WowsKarma.Api.Services.Replays;
using WowsKarma.Common;

namespace WowsKarma.Api.Services;

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
		
		string connectionString = configuration[$"API:{Startup.ApiRegion.ToRegionString()}:Azure:Storage:ConnectionString"] 
			?? throw new ArgumentException("Missing API:{region}:Azure:Storage:ConnectionString configuration value.");
		BlobServiceClient serviceClient = new(connectionString);
		_containerClient = serviceClient.GetBlobContainerClient(MinimapBlobContainer);

		if (!_containerClient.Exists())
		{
			_containerClient.Create(PublicAccessType.Blob);
		}
	}

	/// <summary>
	/// Renders the minimap for the specified post's replay.
	/// </summary>
	/// <param name="postId">The ID of the post to render the minimap for.</param>
	/// <param name="force">Whether to force rendering the minimap, even if it has already been rendered.</param>
	/// <param name="ct">The cancellation token.</param>
	[Tag("minimap", "replay", "render", "single"), JobDisplayName("Render replay minimap for post {0}")]
	public async Task RenderPostReplayMinimapAsync(Guid postId, bool force = false, CancellationToken ct = default)
	{
		_logger.LogDebug("Rendering minimap for post {postId}.", postId);
		
		Post post = await _context.Posts.Include(static r => r.Replay).FirstOrDefaultAsync(p => p.Id == postId, cancellationToken: ct) 
			?? throw new ArgumentException($"Post with ID {postId} does not exist.", nameof(postId));

		if (!force && post.Replay is null or { MinimapRendered: true })
		{
			_logger.LogInformation("Skipping minimap rendering for post {postId}.", postId);
            return;
		}
		
		await using MemoryStream ms = await _replaysIngestService.FetchReplayFileAsync(post.Replay.Id, ct);
		ms.Position = 0;
		
		_logger.LogDebug("Rendering minimap for replay {replayId} from post {postId}. Target player ID: {targetPlayerId}", post.Replay.Id, postId, post.PlayerId);
		byte[] response = await _client.RenderReplayMinimapAsync(ms.ToArray(), post.Replay.Id.ToString(), post.PlayerId, ct);
		
        _logger.LogInformation("Minimap rendered for replay {replayId} from post {postId}.", post.Replay.Id, postId);
		await UploadReplayMinimapAsync(post.Replay.Id, response, force, ct);
		
		post.Replay.MinimapRendered = true;
		await _context.SaveChangesAsync(ct);
	}

	/// <summary>
	/// Uploads the rendered minimap for the specified post's replay to Azure storage.
	/// </summary>
	/// <param name="replayId">The ID of the replay to upload the minimap for.</param>
	/// <param name="content">The minimap video as a blob.</param>
	/// <param name="force">Whether to force uploading the minimap, even if it has already been uploaded.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <exception cref="ArgumentException">Thrown when no post was found.</exception>
	private async ValueTask UploadReplayMinimapAsync(Guid replayId, byte[] content, bool force, CancellationToken ct = default)
	{
		_logger.LogDebug("Uploading minimap for replay {replayId} to Azure storage.", replayId);
		
		Post post = await _context.Posts.Include(static r => r.Replay).FirstOrDefaultAsync(p => p.Replay.Id == replayId, ct) 
			?? throw new ArgumentException($"Post with replay ID {replayId} does not exist.", nameof(replayId));

		if (!force && post.Replay is null or { MinimapRendered: true })
		{
			_logger.LogInformation("Skipping minimap upload for replay {replayId}.", replayId);
			return;
		}
		
		await using MemoryStream ms = new(content);
		ms.Position = 0;

		
		await _containerClient.GetBlobClient($"{post.Replay.Id}.mp4").UploadAsync(ms, overwrite: true, cancellationToken: ct);
		_logger.LogInformation("Minimap uploaded for replay {replayId} to Azure storage.", replayId);
	}

	/// <summary>
	/// Triggers minimap rendering on all replays between the specified dates.
	/// </summary>
	/// <param name="start">The start date.</param>
	/// <param name="end">The end date.</param>
	/// <param name="force">Whether to force rendering a minimap, even if it has already been rendered.</param>
	/// <param name="ct">The cancellation token.</param>
	[Tag("minimap", "replay", "render", "batch"), JobDisplayName("Render replay minimaps between {0} and {1}")]
	public async Task ReprocessAllMinimapsAsync(DateTime? start, DateTime? end, bool force = false, CancellationToken ct = default)
	{
		_logger.LogWarning("Started rendering all replay minimaps between {start:g} and {end:g}", start, end);

		IQueryable<Post> posts = _context.Posts.Include(static p => p.Replay)
			.Where(p => p.Replay != null && p.CreatedAt >= start && p.CreatedAt <= end)
			.OrderByDescending(p => p.CreatedAt);

		int postsCount = await posts.CountAsync(ct);
		
		_logger.LogWarning("Database readout complete. {count} replay minimaps will be rendered.", postsCount);
        
		await foreach (Post post in posts.AsAsyncEnumerable().WithCancellation(ct))
		{
			try
			{
				await RenderPostReplayMinimapAsync(post.Id, force, ct);
			}
			catch (Exception e)
			{
				_logger.LogWarning(e, "Failed to render minimap for replay {replayId} from post {postId}. Skipping.", post.Replay.Id, post.Id);
			}
		}
		
		_logger.LogWarning("Replay Minimaps rendering complete! Rendered {count} replay minimaps total.", postsCount);
	}
	
	public async ValueTask<Uri> GenerateMinimapUriAsync(Guid replayId)
	{
		Replay replay = await _context.Replays.FindAsync(replayId) ?? throw new ArgumentException("No replay was found for specified GUID.", nameof(replayId));
		return _containerClient.GetBlobClient(replay.BlobName).Uri;
	}
}