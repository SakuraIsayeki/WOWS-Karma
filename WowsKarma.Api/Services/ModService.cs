using System.Diagnostics;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Services.Discord;
using WowsKarma.Api.Services.Posts;

namespace WowsKarma.Api.Services;

public sealed class ModService
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

	public Task<PostModAction?> GetModActionAsync(Guid id) => _context.PostModActions.AsNoTracking().FirstOrDefaultAsync(ma => ma.Id == id);

	public IQueryable<PostModAction> GetPostModActions(Guid postId) => _context.PostModActions.AsNoTracking()
		.Include(ma => ma.Post)
		.Include(ma => ma.Mod)
		.Where(ma => ma.PostId == postId);

	public IQueryable<PostModAction> GetPostModActions(uint playerId) => _context.PostModActions.AsNoTracking()
		.Include(ma => ma.Post)
		.Include(ma => ma.Mod)
		.Where(ma => ma.Post.AuthorId == playerId);

	public Task<PlatformBan?> GetPlatformBan(Guid id) => _context.PlatformBans.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

	public IQueryable<PlatformBan> GetPlatformBans(uint userId) => _context.PlatformBans.AsNoTracking().Where(b => b.UserId == userId);

	public async Task SubmitPostModActionAsync(PostModActionDTO modAction)
	{
		EntityEntry<PostModAction> entityEntry = await _context.PostModActions.AddAsync(modAction.Adapt<PostModAction>());
		await _context.SaveChangesAsync();
		
		switch (modAction.ActionType)
		{
			case ModActionType.Deletion:
			{
				await _postService.DeletePostAsync(modAction.PostId, true);

				using PostModDeletedNotification notification = PostModDeletedNotification.FromModAction(entityEntry.Entity);
				await _notifications.SendNewNotification(notification);
				break;
			}

			case ModActionType.Update:
			{
				PlayerPostDTO current = _postService.GetPost(modAction.PostId).Adapt<PlayerPostDTO>();

				await _postService.EditPostAsync(modAction.PostId, current with
				{
					Title = modAction.UpdatedPost?.Title ?? current.Title, 
					Content = modAction.UpdatedPost?.Content ?? current.Content,
					Flairs = modAction.UpdatedPost?.Flairs ?? current.Flairs
				}, true);

				break;
			}

			default: throw new UnreachableException();
		}

		await entityEntry.Reference(pma => pma.Mod).LoadAsync();
		await entityEntry.Reference(pma => pma.Post).Query().Include(p => p.Author).LoadAsync();

		await _webhookService.SendModActionWebhookAsync(entityEntry.Entity);
	}

	public async Task RevertModActionAsync(Guid modActionId)
	{
		using PostModAction modAction = await _context.PostModActions.FindAsync(modActionId) ?? throw new ArgumentException($"Mod action with id {modActionId} not found");

		_context.PostModActions.Remove(modAction);
		await _postService.RevertPostModLockAsync(modAction.PostId);
		await _context.SaveChangesAsync();
		
		await _webhookService.SendModActionRevertedWebhookAsync(modAction);
	}

	public async Task EmitPlatformBanAsync(PlatformBanDTO platformBan, [FromServices] AuthDbContext authContext)
	{
		_ = platformBan ?? throw new ArgumentNullException(nameof(platformBan));
		
		EntityEntry<PlatformBan> entityEntry = _context.PlatformBans.Add(new()
		{
			UserId = platformBan.UserId,
			Reason = platformBan.Reason,
			BannedUntil = platformBan.BannedUntil,
			ModId = platformBan.ModId,
		});

		await _context.SaveChangesAsync();
		
		const string logFormat = "Platform banned user {userId} until {until} for reason \"{reason}\".";

		if (await authContext.Users.AnyAsync(u => u.Id == platformBan.UserId))
		{
			_logger.LogInformation(logFormat, platformBan.UserId, platformBan.BannedUntil as object ?? "Indefinitely", platformBan.Reason);
		}
		else
		{
			_logger.LogWarning(logFormat + " However the user has never logged onto the platform before.",
				platformBan.Reason, platformBan.BannedUntil as object ?? "Indefinitely", platformBan.Reason);
		}

		await entityEntry.Reference(b => b.Mod).LoadAsync();
		await entityEntry.Reference(b => b.User).LoadAsync();

		await _webhookService.SendPlatformBanWebhookAsync(entityEntry.Entity);
		await _notifications.SendNewNotification(new PlatformBanNotification
		{
			AccountId = entityEntry.Entity.UserId,
			BanId = entityEntry.Entity.Id
		});
	}

	public async Task RevertPlatformBanAsync(Guid id)
	{
		PlatformBan ban = await _context.PlatformBans.FindAsync(id) ?? throw new ArgumentException($"Platform ban with id {id} not found");
		ban.Reverted = true;
		await _context.SaveChangesAsync();
		_logger.LogInformation("Reverted Ban {banId}.", ban.Id);
	}
}
