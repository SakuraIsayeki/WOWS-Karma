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
			await Clients.Caller.SendAsync("GetLatestPosts", postService.GetLatestPosts(count));
		}

		public async Task NewPost(PlayerPostDTO postDTO)
		{
			await Clients.All.SendAsync("NewPost", postDTO);
		}
	}
}
