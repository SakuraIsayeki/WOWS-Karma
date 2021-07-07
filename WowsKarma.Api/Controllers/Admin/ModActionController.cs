using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Services;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Controllers.Admin
{
	[ApiController, Route("api/mod/action"), Authorize(Roles = ApiRoles.CM)]
	public class ModActionController : ControllerBase
	{
		private readonly ModService service;

		public ModActionController(ModService service)
		{
			this.service = service;
		}

		[HttpGet("{id}"), AllowAnonymous]
		public Task<PostModAction> Fetch(Guid id) => service.GetModActionAsync(id);

		[HttpGet("list"), AllowAnonymous]
		public IActionResult List([FromQuery] Guid postId = default, [FromQuery] uint userId = default)
		{
			IEnumerable<PostModAction> modActions;

			if (postId != default)
			{
				modActions = service.GetPostModActions(postId);
			}
			else if (userId is not 0)
			{
				modActions = service.GetPostModActions(userId);
			}
			else
			{
				return BadRequest("Please use a search query (Post/User).");
			}

			return modActions?.Count() is null or 0
				? StatusCode(204)
				: StatusCode(200, modActions);
		}

		[HttpPost, Authorize(Roles = ApiRoles.CM)]
		public async Task<IActionResult> Submit([FromBody] PostModActionDTO modAction)
		{
			uint modId = uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
			await service.SubmitModActionAsync(modAction with { ModId = modId });
			return StatusCode(202);
		}

		[HttpDelete("{id}"), Authorize(Roles = ApiRoles.CM)]
		public async Task<IActionResult> Revert(Guid id)
		{
			await service.RevertModActionAsync(id);
			return StatusCode(205);
		}
	}
}
