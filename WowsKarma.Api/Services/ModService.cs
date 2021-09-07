using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Services.Discord;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Services
{
	public class ModService
	{
		private readonly ILogger<ModService> logger;
		private readonly ModActionWebhookService webhookService;
		private readonly PostService postService;
		private readonly ApiDbContext context;

		public ModService(ILogger<ModService> logger, IDbContextFactory<ApiDbContext> dbContextFactory, ModActionWebhookService webhookService, PostService postService)
		{
			context = dbContextFactory.CreateDbContext();
			this.logger = logger;
			this.webhookService = webhookService;
			this.postService = postService;
		}

		public Task<PostModAction> GetModActionAsync(Guid id) => context.PostModActions.AsNoTracking().FirstOrDefaultAsync(ma => ma.Id == id);

		public IQueryable<PostModAction> GetPostModActions(Guid postId)	=> context.PostModActions.AsNoTracking()
			.Include(ma => ma.Post)
			.Include(ma => ma.Mod)
			.Where(ma => ma.PostId == postId);

		public IQueryable<PostModAction> GetPostModActions(uint playerId) => context.PostModActions.AsNoTracking()
			.Include(ma => ma.Post)
			.Include(ma => ma.Mod)
			.Where(ma => ma.Post.AuthorId == playerId);

		public async Task SubmitModActionAsync(PostModActionDTO modAction)
		{
			EntityEntry<PostModAction> entityEntry = await context.PostModActions.AddAsync(modAction.Adapt<PostModAction>());

			switch (modAction.ActionType)
			{
				case ModActionType.Deletion:
					await postService.DeletePostAsync(modAction.PostId, true);
					break;

				case ModActionType.Update:
					PlayerPostDTO current = postService.GetPost(modAction.PostId).Adapt<PlayerPostDTO>();

					await postService.EditPostAsync(modAction.PostId, current with
					{
						Content = modAction.UpdatedPost.Content ?? current.Content,
						Flairs = modAction.UpdatedPost.Flairs
					});

					break;
			}

			await context.SaveChangesAsync();

			await entityEntry.Reference(pma => pma.Mod).LoadAsync();
			await entityEntry.Reference(pma => pma.Post).Query().Include(p => p.Author).LoadAsync();

			_ = webhookService.SendModActionWebhookAsync(entityEntry.Entity);
		}

		public Task RevertModActionAsync(Guid modActionId)
		{
			PostModAction stub = new() { Id = modActionId };

			context.PostModActions.Attach(stub);
			context.PostModActions.Remove(stub);

			return context.SaveChangesAsync();
		}
	}
}
