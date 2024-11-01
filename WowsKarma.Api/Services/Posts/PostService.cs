using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Services.Replays;

namespace WowsKarma.Api.Services.Posts;

public sealed class PostService
{
	public const ushort PostTitleMaxSize = 60;
	public const ushort PostContentMaxSize = 2000;
	public static TimeSpan CooldownPeriod { get; } = TimeSpan.FromDays(1);

	private readonly ApiDbContext _context;
	private readonly PlayerService _playerService;
	private readonly ReplaysIngestService _replayService;

	public PostService(ApiDbContext context, PlayerService playerService, ReplaysIngestService replayService)
	{
		_context = context;
		_playerService = playerService;
		_replayService = replayService;
	}

	/// <summary>
	/// Lists all post IDs.
	/// </summary>
	/// <returns>Async enumerable of post IDs.</returns>
	public IAsyncEnumerable<Guid> ListPostIdsAsync() => _listPostIdsAsync(_context);
	private static readonly Func<ApiDbContext, IAsyncEnumerable<Guid>> _listPostIdsAsync = EF.CompileAsyncQuery(
		(ApiDbContext context) => context.Posts.OrderBy(p => p.CreatedAt).Select(p => p.Id)
	);

	/// <summary>
	/// Gets a post by its ID.
	/// </summary>
	/// <param name="id">The post's ID.</param>
	/// <returns>The post, or <see langword="null"/> if not found.</returns>
	public Post? GetPost(Guid id) => GetPost(_context, id);
	internal static Post? GetPost(ApiDbContext context, Guid id) => context.Posts
		.Include(p => p.Author)
			.ThenInclude(p => p.ClanMember)
			.ThenInclude(p => p.Clan)
			
		.Include(p => p.Player)
			.ThenInclude(p => p.ClanMember)
			.ThenInclude(p => p.Clan)
			
		.Include(p => p.Replay)
		.FirstOrDefault(p => p.Id == id);

	/// <summary>
	/// Gets a post's DTO by its ID.
	/// </summary>
	/// <param name="id">The post's ID.</param>
	/// <returns>The post, or <see langword="null"/> if not found.</returns>
	public async Task<PlayerPostDTO?> GetPostDTOAsync(Guid id)
	{
		if (GetPost(id) is not { } post)
		{
			return null;
		}
			
		PlayerPostDTO postDto = post.Adapt<PlayerPostDTO>();

		return post.ReplayId is null 
			? postDto 
			: postDto with { Replay = await _replayService.GetReplayDTOAsync(post.ReplayId.Value) };
	}

	public IQueryable<Post> GetReceivedPosts(uint playerId) => _context.Posts.AsNoTracking()
		.Include(p => p.Author)
			.ThenInclude(p => p.ClanMember)
			.ThenInclude(p => p.Clan)
			
		.Include(p => p.Player)
			.ThenInclude(p => p.ClanMember)
			.ThenInclude(p => p.Clan)
		
		.Include(p => p.Replay)
		
		.Where(p => p.PlayerId == playerId)
		.OrderByDescending(p => p.CreatedAt);

	public IQueryable<Post> GetSentPosts(uint authorId) => _context.Posts.AsNoTracking()
		.Include(p => p.Author)
			.ThenInclude(p => p.ClanMember)
			.ThenInclude(p => p.Clan)
			
		.Include(p => p.Player)
			.ThenInclude(p => p.ClanMember)
			.ThenInclude(p => p.Clan)
		
		.Include(p => p.Replay)
		
		.Where(p => p.AuthorId == authorId)
		.OrderByDescending(p => p.CreatedAt);

	public IQueryable<Post> GetLatestPosts() => _context.Posts.AsNoTracking()
		.Include(p => p.Author)
			.ThenInclude(p => p.ClanMember)
			.ThenInclude(p => p.Clan)
			
		.Include(p => p.Player)
			.ThenInclude(p => p.ClanMember)
			.ThenInclude(p => p.Clan)
			
		.OrderByDescending(p => p.CreatedAt);

	public async Task<Post> CreatePostAsync(PlayerPostDTO postDto, IFormFile? replayFile, bool bypassChecks)
	{
		bool hasReplay = replayFile is not null;

		Task<Replay>? replayIngestTask = hasReplay ? _replayService.IngestReplayAsync(replayFile, CancellationToken.None) : null;

		try
		{
			ValidatePostContents(postDto);
		}
		catch (Exception e)
		{
			throw new ArgumentException("Validation failed.", nameof(postDto), e);
		}

		Player author = await _playerService.GetPlayerAsync(postDto.Author.Id) ?? throw new ArgumentException($"Author Account {postDto.Author.Id} not found", nameof(postDto));
		Player player = await _context.Players.FindAsync(postDto.Player.Id) ?? throw new ArgumentException($"Player Account {postDto.Player.Id} not found", nameof(postDto));

		if (!bypassChecks)
		{
			if (player.OptedOut)
			{
				throw new ArgumentException("Player has opted-out of this platform.");
			}

			if (CheckCooldown(postDto))
			{
				throw new CooldownException("Author is on cooldown for this player.");
			}
		}

		using Post post = postDto.Adapt<Post>();
		post.NegativeKarmaAble = author.NegativeKarmaAble;

		EntityEntry<Post> entry = _context.Posts.Add(post);
		KarmaService.UpdatePlayerKarma(player, post.ParsedFlairs, null, post.NegativeKarmaAble);
		KarmaService.UpdatePlayerRatings(player, post.ParsedFlairs, null);

		if (hasReplay)
		{
			using Replay replay = await replayIngestTask!;

			entry.Entity.ReplayId = replay.Id;
			entry.Entity.Replay = replay; 
		}

		await _context.SaveChangesAsync();

		// Queue background notification jobs
		PostUpdatesBroadcastService.OnPostCreationAsync(post.Id);
		
		return entry.Entity;
	}

	public async Task EditPostAsync(Guid id, PlayerPostDTO edited, bool modEditLock = false)
	{
		ValidatePostContents(edited);

		Post current = await _context.Posts.FindAsync(id) ?? throw new ArgumentException($"Post {id} not found", nameof(id));
		PostFlairsParsed? previousFlairs = current.ParsedFlairs;
		Player player = await _context.Players.FindAsync(current.PlayerId) ?? throw new ArgumentException($"Player Account {edited.Player.Id} not found", nameof(edited));

		current.Title = edited.Title;
		current.Content = edited.Content;
		current.Flairs = edited.Flairs;
		current.UpdatedAt = DateTimeOffset.UtcNow; // Forcing UpdatedAt refresh
		current.ReadOnly = current.ReadOnly || modEditLock;
		current.CustomerSupportTicketId = edited.SupportTicketStatus.TicketId;

		KarmaService.UpdatePlayerKarma(player, current.ParsedFlairs, previousFlairs, current.NegativeKarmaAble);
		KarmaService.UpdatePlayerRatings(player, current.ParsedFlairs, previousFlairs);

		await _context.SaveChangesAsync();

		// Queue background notification jobs
		PostUpdatesBroadcastService.OnPostUpdateAsync(id);
	}

	public async Task DeletePostAsync(Guid id, bool modLock = false)
	{
		using Post post = await _context.Posts.FindAsync(id) ?? throw new ArgumentException($"Post {id} not found", nameof(id));
		Player player = await _context.Players.FindAsync(post.PlayerId) ?? throw new ArgumentException($"Player Account {post.PlayerId} not found", nameof(id));

		if (modLock)
		{
			post.ModLocked = true;
		}
		else
		{
			_context.Posts.Remove(post);
		}

		KarmaService.UpdatePlayerKarma(player, null, post.ParsedFlairs, post.NegativeKarmaAble);
		KarmaService.UpdatePlayerRatings(player, null, post.ParsedFlairs);

		await _context.SaveChangesAsync();

		// Queue background notification jobs
		PostUpdatesBroadcastService.OnPostDeletionAsync(post.Adapt<PlayerPostDTO>(), modLock);
	}

	public async Task RevertPostModLockAsync(Guid id)
	{
		using Post post = await _context.Posts.FindAsync(id) ?? throw new ArgumentException($"Post {id} not found", nameof(id));
		post.ModLocked = false;

		Player player = await _context.Players.FindAsync(post.PlayerId) ?? throw new ArgumentException($"Player Account {post.PlayerId} not found");
		KarmaService.UpdatePlayerKarma(player, post.ParsedFlairs, null, post.NegativeKarmaAble);
		KarmaService.UpdatePlayerRatings(player, post.ParsedFlairs, null);

		await _context.SaveChangesAsync();
	}

	internal static void ValidatePostContents(PlayerPostDTO post)
	{
		_ = post ?? throw new ArgumentNullException(nameof(post));

		_ = string.IsNullOrWhiteSpace(post.Title) ? throw new ArgumentException("Post Title is Empty", nameof(post)) : false;
		_ = string.IsNullOrWhiteSpace(post.Content) ? throw new ArgumentException("Post Content is Empty", nameof(post)) : false;

		_ = post.Title.Length > PostTitleMaxSize ? throw new ArgumentException($"Post Title Length exceeds {PostTitleMaxSize} characters", nameof(post)) : false;
		_ = post.Content.Length > PostContentMaxSize ? throw new ArgumentException($"Post Content Length exceeds {PostContentMaxSize} characters", nameof(post)) : false;
	}


	private bool CheckCooldown(PlayerPostDTO post)
	{
		IQueryable<Post> filteredPosts =
			from p in _context.Posts
			where p.AuthorId == post.Author.Id
			where p.PlayerId == post.Player.Id
			select p;

		if (filteredPosts.Any())
		{
			PlayerPostDTO? lastAuthoredPost = filteredPosts.OrderBy(p => p.CreatedAt).LastOrDefault()?.Adapt<PlayerPostDTO>();

			if (lastAuthoredPost is { CreatedAt: not null })
			{
				DateTimeOffset endsAt = lastAuthoredPost.CreatedAt.Value.Add(CooldownPeriod);
				return endsAt > DateTimeOffset.UtcNow;
			}
		}

		return false;
	}
}