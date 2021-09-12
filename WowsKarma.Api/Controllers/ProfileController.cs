using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Services;
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
			? StatusCode(200, player.Adapt<UserProfileFlagsDTO>())
			: StatusCode(404);

		[HttpPut, Authorize]
		public async Task<IActionResult> UpdateProfileFlagsAsync([FromBody] UserProfileFlagsDTO flags)
		{
			try
			{
				if (flags.Id != User.ToAccountListing().Id && !User.IsInRole(ApiRoles.Administrator))
				{
					return StatusCode(403, "User can only update their own profile.");
				}

				await playerService.UpdateProfileFlagsAsync(flags);
				return StatusCode(200);
			}
			catch (CooldownException e)
			{
				return StatusCode(423, e);
			}
			catch (ArgumentException)
			{
				return StatusCode(404);
			}
		}
	}
}
