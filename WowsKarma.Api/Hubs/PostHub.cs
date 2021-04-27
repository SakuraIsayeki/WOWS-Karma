using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Services;
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
			List<PlayerPostDTO> postsDTOs = new();
			foreach (Post post in postService.GetLatestPosts(count))
			{
				postsDTOs.Add(post);
			}

			await Clients.Caller.SendAsync("GetLatestPosts", postsDTOs);
		}

		internal async Task NewPost(PlayerPostDTO postDTO) => await Clients.All.SendAsync("NewPost", postDTO);

		internal async Task EditedPost(PlayerPostDTO edited) => await Clients.All.SendAsync("EditedPost", edited);

		internal async Task DeletedPost(Guid postId) => await Clients.All.SendAsync("DeletedPost", postId);
	}
}
