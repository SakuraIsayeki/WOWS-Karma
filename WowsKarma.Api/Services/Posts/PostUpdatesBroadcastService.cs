using Hangfire;
using Hangfire.Tags.Attributes;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Hubs;
using WowsKarma.Api.Services.Discord;
using WowsKarma.Common.Hubs;


#nullable enable
namespace WowsKarma.Api.Services.Posts;

// ReSharper disable MemberCanBePrivate.Global

/// <summary>
/// Annex service to the posts service, deals with broadcasting post updates and notifications to the SignalR hubs,
/// as well as publishing updates
/// </summary>
public class PostUpdatesBroadcastService
{
	private readonly ApiDbContext _dbContext;
	private readonly IHubContext<PostHub, IPostHubPush> _hubContext;
	private readonly NotificationService _notificationService;
	private readonly PostWebhookService _webhookService;

	public PostUpdatesBroadcastService(ApiDbContext dbContext, IHubContext<PostHub, IPostHubPush> hubContext, 
		NotificationService notificationService, PostWebhookService webhookService)
	{
		_dbContext = dbContext;
		_hubContext = hubContext;
		_notificationService = notificationService;
		_webhookService = webhookService;
	}

	/// <summary>
	/// Triggers all the necessary actions to broadcast a post creation to the SignalR hubs and Discord webhooks
	/// </summary>
	/// <param name="postId">The post id.</param>
	public static void OnPostCreationAsync(Guid postId)
	{
		BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.LogPostCreationAsync(postId));
		BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.BroadcastPostCreationAsync(postId));
		BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.NotifyPostCreationAsync(postId));
	}
	
	/// <summary>
	/// Triggers all the necessary actions to broadcast a post update to the SignalR hubs and Discord webhooks
	/// </summary>
	/// <param name="postId">The post id.</param>
	public static void OnPostUpdateAsync(Guid postId)
	{
		BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.LogPostEditionAsync(postId));
		BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.BroadcastPostEditionAsync(postId));
		BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.NotifyPostEditionAsync(postId));
	}

	/// <summary>
	/// Triggers all the necessary actions to broadcast a post deletion to the SignalR hubs and Discord webhooks
	/// </summary>
	/// <param name="postId">The post id.</param>
	public static void OnPostDeletionAsync(Guid postId, bool modlock)
	{
		if (!modlock)
		{
			BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.LogPostDeletionAsync(postId));
		}
		
		BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.BroadcastPostDeletionAsync(postId));
		BackgroundJob.Enqueue<PostUpdatesBroadcastService>(s => s.NotifyPostDeletionAsync(postId));
	}
	
	#region Creation
	
	[Tag("post", "creation", "log", "webhook"), JobDisplayName("Log player post creation through webhook")]
	public async Task LogPostCreationAsync(Guid postId)
	{
		// Get the post from the database, and adapt to DTO.
		Post post = PostService.GetPost(_dbContext, postId);
		PlayerPostDTO postDto = post.Adapt<PlayerPostDTO>();
		
		// Send the webhook.
		await _webhookService.SendNewPostWebhookAsync(postDto);
	}
	
	[Tag("post", "creation", "broadcast", "signalr"), JobDisplayName("Broadcast player post creation on posts hub")]
	public async Task BroadcastPostCreationAsync(Guid postId)
	{
		// Get the post from the database, and adapt to DTO.
		Post post = PostService.GetPost(_dbContext, postId);
		PlayerPostDTO postDto = post.Adapt<PlayerPostDTO>();
		
		// Send the update to the clients.
		await _hubContext.Clients.All.NewPost(postDto);
	}
	
	[Tag("post", "creation", "notification", "webhook"), JobDisplayName("Notify player post creation on notifications hub")]
	public async Task NotifyPostCreationAsync(Guid postId)
	{
		// Get the post from the database, and adapt to DTO.
		Post post = PostService.GetPost(_dbContext, postId);

		// Send the notification.
		await _notificationService.SendNewNotification(new PostAddedNotification
		{
			AccountId = post.Player.Id,
			PostId = post.Id
		});
	}
	
	#endregion

	#region Edition

	[Tag("post", "edition", "log", "webhook"), JobDisplayName("Log player post edition through webhook")]
	public async Task LogPostEditionAsync(Guid postId)
	{
		// Get the post from the database, and adapt to DTO.
		Post post = PostService.GetPost(_dbContext, postId);
		PlayerPostDTO postDto = post.Adapt<PlayerPostDTO>();
		
		// Send the webhook.
		await _webhookService.SendEditedPostWebhookAsync(postDto);
	}

	[Tag("post", "edition", "broadcast", "signalr"), JobDisplayName("Broadcast player post edition on posts hub")]
	public async Task BroadcastPostEditionAsync(Guid postId)
	{
		// Get the post from the database, and adapt to DTO.
		Post post = PostService.GetPost(_dbContext, postId);
		PlayerPostDTO postDto = post.Adapt<PlayerPostDTO>();
		
		// Send the update to the clients.
		await _hubContext.Clients.All.EditedPost(postDto);
	}
	
	[Tag("post", "edition", "notification", "webhook"), JobDisplayName("Notify player post edition on notifications hub")]
	public async Task NotifyPostEditionAsync(Guid postId)
	{
		// Get the post from the database, and adapt to DTO.
		Post post = PostService.GetPost(_dbContext, postId);

		// Send the notification.
		await _notificationService.SendNewNotification(new PostEditedNotification
		{
			AccountId = post.Player.Id,
			PostId = post.Id
		});
	}

	#endregion
	
	#region Deletion
	
	[Tag("post", "deletion", "log", "webhook"), JobDisplayName("Log player post deletion through webhook")]
	public async Task LogPostDeletionAsync(Guid postId)
	{
		// Get the post from the database, and adapt to DTO.
		Post post = PostService.GetPost(_dbContext, postId);
		PlayerPostDTO postDto = post.Adapt<PlayerPostDTO>();
		
		// Send the webhook.
		await _webhookService.SendDeletedPostWebhookAsync(postDto);
	}
	
	[Tag("post", "deletion", "broadcast", "signalr"), JobDisplayName("Broadcast player post deletion on posts hub")]
	public async Task BroadcastPostDeletionAsync(Guid postId)
	{
		// Send the update to the clients.
		await _hubContext.Clients.All.DeletedPost(postId);
	}
	
	[Tag("post", "deletion", "notification", "webhook"), JobDisplayName("Notify player post deletion on notifications hub")]
	public async Task NotifyPostDeletionAsync(Guid postId)
	{
		// Get the post from the database, and adapt to DTO.
		Post post = PostService.GetPost(_dbContext, postId);

		// Send the notification.
		await _notificationService.SendNewNotification(new PostDeletedNotification
		{
			AccountId = post.Player.Id,
			PostId = post.Id
		});
	}
	
	#endregion
}