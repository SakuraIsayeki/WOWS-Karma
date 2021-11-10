using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Services.Discord;



namespace WowsKarma.Api.Services;

public class ModService
{
	private readonly ILogger<ModService> _logger;
	private readonly ModActionWebhookService _webhookService;
	private readonly PostService _postService;
	private readonly NotificationService _notifications;
	private readonly ApiDbContext _context;

	public ModService(ILogger<ModService> logger, ApiDbContext context, ModActionWebhookService webhookService, PostService postService, NotificationService notifications)
	{
		_context = context;
		_logger = logger;
		_webhookService = webhookService;
		_postService = postService;
		_notifications = notifications;
	}

	public Task<PostModAction> GetModActionAsync(Guid id) => _context.PostModActions.AsNoTracking().FirstOrDefaultAsync(ma => ma.Id == id);

	public IQueryable<PostModAction> GetPostModActions(Guid postId) => _context.PostModActions.AsNoTracking()
		.Include(ma => ma.Post)
		.Include(ma => ma.Mod)
		.Where(ma => ma.PostId == postId);

	public IQueryable<PostModAction> GetPostModActions(uint playerId) => _context.PostModActions.AsNoTracking()
		.Include(ma => ma.Post)
		.Include(ma => ma.Mod)
		.Where(ma => ma.Post.AuthorId == playerId);

	public Task<PlatformBan> GetPlatformBan(Guid id) => _context.PlatformBans.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

	public IQueryable<PlatformBan> GetPlatformBans(uint userId) => _context.PlatformBans.AsNoTracking().Where(b => b.UserId == userId);

	public async Task SubmitModActionAsync(PostModActionDTO modAction)
	{
		EntityEntry<PostModAction> entityEntry = await _context.PostModActions.AddAsync(modAction.Adapt<PostModAction>());

		switch (modAction.ActionType)
		{
			case ModActionType.Deletion:
				await _postService.DeletePostAsync(modAction.PostId, true);
				await _notifications.SendNewNotification(PostModDeletedNotification.FromModAction(entityEntry.Entity));
				break;

			case ModActionType.Update:
				PlayerPostDTO current = _postService.GetPost(modAction.PostId).Adapt<PlayerPostDTO>();

				await _postService.EditPostAsync(modAction.PostId, current with
				{
					Content = modAction.UpdatedPost.Content ?? current.Content,
					Flairs = modAction.UpdatedPost.Flairs
				});

				break;
		}

		await _context.SaveChangesAsync();

		await entityEntry.Reference(pma => pma.Mod).LoadAsync();
		await entityEntry.Reference(pma => pma.Post).Query().Include(p => p.Author).LoadAsync();

		_ = _webhookService.SendModActionWebhookAsync(entityEntry.Entity);
	}

	public Task RevertModActionAsync(Guid modActionId)
	{
		PostModAction stub = new() { Id = modActionId };

		_context.PostModActions.Attach(stub);
		_context.PostModActions.Remove(stub);

		return _context.SaveChangesAsync();
	}

	public async Task EmitPlatformBanAsync(PlatformBanDTO platformBan, [FromServices] AuthDbContext authContext)
	{
		_ = platformBan ?? throw new ArgumentNullException(nameof(platformBan));

		Task<bool> userHasLoggedInBefore = authContext.Users.AnyAsync(u => u.Id == platformBan.UserId);

		EntityEntry<PlatformBan> entityEntry = _context.PlatformBans.Add(new()
		{
			UserId = platformBan.UserId,
			Reason = platformBan.Reason,
			BannedUntil = platformBan.BannedUntil,
			ModId = platformBan.ModId,
		});

		await _context.SaveChangesAsync();

		Task refs = Task.WhenAll(
			entityEntry.Reference(b => b.Mod).LoadAsync(),
			entityEntry.Reference(b => b.User).LoadAsync());

		const string logFormat = "Platform banned user {userId} until {until} for reason \"{reason}\".";

		if (await userHasLoggedInBefore)
		{
			_logger.LogInformation(logFormat, platformBan.UserId, platformBan.BannedUntil as object ?? "Indefinitely", platformBan.Reason);
		}
		else
		{
			_logger.LogWarning(logFormat + " However the user has never logged onto the platform before.",
				platformBan.Reason, platformBan.BannedUntil as object ?? "Indefinitely", platformBan.Reason);
		}

		await refs;

		await _webhookService.SendPlatformBanWebhookAsync(entityEntry.Entity);
	}
}
