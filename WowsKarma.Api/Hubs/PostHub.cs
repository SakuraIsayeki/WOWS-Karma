using Mapster;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Services;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Hubs
{
	public class PostHub : Hub
	{
		private readonly PostService postService;

		public PostHub(PostService postService)
		{
			this.postService = postService;
		}

		public async Task GetLatestPosts(int count)
		{
			AccountListingDTO currentUser = Context.User.ToAccountListing();
			List<PlayerPostDTO> postsDTOs = new(postService.GetLatestPosts(count).Where(p => !p.ModLocked || p.AuthorId == currentUser.Id).Adapt<IEnumerable<PlayerPostDTO>>());
			await Clients.Caller.SendAsync("GetLatestPosts", postsDTOs);
		}

		internal async Task NewPost(PlayerPostDTO postDTO) => await Clients.All.SendAsync("NewPost", postDTO);

		internal async Task EditedPost(PlayerPostDTO edited) => await Clients.All.SendAsync("EditedPost", edited);

		internal async Task DeletedPost(Guid postId) => await Clients.All.SendAsync("DeletedPost", postId);
	}
}
