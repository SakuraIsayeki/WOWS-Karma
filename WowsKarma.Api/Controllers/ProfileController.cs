using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class ProfileController : ControllerBase
	{
		private readonly PlayerService playerService;

		public ProfileController(PlayerService playerService)
		{
			this.playerService = playerService;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetProfileFlagsAsync(uint id) => (await playerService.GetPlayerAsync(id)) is Player player
			? StatusCode(200, new UserProfileFlagsDTO
			{
				Id = player.Id,
				PostsBanned = player.PostsBanned,
				OptedOut = player.OptedOut
			})
			: StatusCode(404);

		[HttpPut, AccessKey]
		public async Task<IActionResult> UpdateProfileFlagsAsync([FromBody] UserProfileFlagsDTO flags)
		{
			try
			{
				await playerService.UpdateProfileFlagsAsync(flags);
				return StatusCode(200);
			}
			catch (ArgumentException)
			{
				return StatusCode(404);
			}
		}
	}
}
