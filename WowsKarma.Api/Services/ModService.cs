using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Services
{
	public class ModService
	{
		private readonly ILogger<ModService> logger;
		private readonly PostService postService;
		private readonly ApiDbContext context;

		public ModService(ILogger<ModService> logger, IDbContextFactory<ApiDbContext> dbContextFactory, PostService postService)
		{
			context = dbContextFactory.CreateDbContext();
			this.logger = logger;
			this.postService = postService;
		}

		public Task<PostModAction> GetModActionAsync(Guid id) => context.PostModActions.FirstOrDefaultAsync(ma => ma.Id == id);

		public IQueryable<PostModAction> GetPostModActions(Guid postId) => context.PostModActions.Where(ma => ma.PostId == postId);

		public async Task SubmitModAction(PostModActionDTO modAction)
		{
			await context.PostModActions.AddAsync(modAction.Adapt<PostModAction>());

			switch (modAction.ActionType)
			{
				case ModActionType.Deletion:
					await postService.DeletePostAsync(modAction.PostId);
					break;

				case ModActionType.Update:
					PlayerPostDTO current = postService.GetPost(modAction.PostId);

					await postService.EditPostAsync(modAction.PostId, current with
					{
						Content = modAction.UpdatedPost.Content ?? current.Content,
						Flairs = modAction.UpdatedPost.Flairs
					});
					break;
			}
		}
	}
}
