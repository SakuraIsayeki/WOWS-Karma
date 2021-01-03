using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Controllers
{

	[ApiController, Route("api/[controller]")]
	public class PostController : ControllerBase
	{
		private readonly PlayerService playerService;
		private readonly PostService postService;

		public PostController(PlayerService playerService, PostService postService)
		{
			this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
			this.postService = postService ?? throw new ArgumentNullException(nameof(postService));
		}


		[HttpGet("{id}/received")]
		public async Task<IActionResult> GetReceivedPosts(uint id, [FromQuery] int? lastResults)
		{

			if (await playerService.GetPlayerAsync(id) is null)
			{
				return StatusCode(404, $"Account {id} not found");
			}

			IEnumerable<Post> posts = postService.GetReceivedPosts(id, lastResults ?? 0);

			if (posts?.Count() is null or 0)
			{
				return StatusCode(204);
			}

			List<PlayerPostDTO> postsDTOs = new();
			foreach (Post post in posts)
			{
				postsDTOs.Add(post);
			}

			return StatusCode(200, postsDTOs);
		}

		[HttpGet("{id}/sent")]
		public async Task<IActionResult> GetSentPosts(uint id, [FromQuery] int? lastResults)
		{
			if (await playerService.GetPlayerAsync(id) is null)
			{
				return StatusCode(404, $"Account {id} not found");
			}

			IEnumerable<Post> posts = postService.GetSentPosts(id, lastResults ?? 0);

			if (posts?.Count() is null or 0)
			{
				return StatusCode(204);
			}

			List<PlayerPostDTO> postsDTOs = new();
			foreach (Post post in posts)
			{
				postsDTOs.Add(post);
			}

			return StatusCode(200, postsDTOs);
		}

		[HttpPost("{id}"), AccessKey]
		public async Task<IActionResult> CreatePost(uint id, [FromBody] PlayerPostDTO post)
		{
			if (await playerService.GetPlayerAsync(id) is null)
			{
				return StatusCode(404, $"Account {id} not found");
			}

			try
			{
				await postService.CreatePostAsync(post);
				return StatusCode(201);
			}
			catch (ArgumentException e)
			{
				return StatusCode(400, e.ToString());
			}
		}

		[HttpPut("{id}"), AccessKey]
		public async Task<IActionResult> EditPost(uint id, [FromBody] PlayerPostDTO post)
		{
			if (await playerService.GetPlayerAsync(id) is null)
			{
				return StatusCode(404, $"Account {id} not found");
			}

			try
			{
				await postService.EditPostAsync(post.Id ?? default, post);
				return StatusCode(200);
			}
			catch (ArgumentException e)
			{
				return StatusCode(400, e.ToString());
			}
		}

		[HttpDelete("{id}"), AccessKey]
		public async Task<IActionResult> DeletePost(Guid id)
		{
			try
			{
				await postService.DeletePostAsync(id);
				return StatusCode(205);
			}
			catch (ArgumentException e)
			{
				return StatusCode(400, e.ToString());
			}
		}

		[HttpGet("model")]
		public IActionResult GetPostDTOModel() => StatusCode(200, new PlayerPostDTO());
	}
}
