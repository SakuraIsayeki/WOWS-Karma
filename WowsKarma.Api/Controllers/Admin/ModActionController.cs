using Mapster;
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

		/// <summary>
		/// Fetches ModAction by ID.
		/// </summary>
		/// <param name="id">ID of ModAction to fetch.</param>
		[HttpGet("{id}"), AllowAnonymous]
		public Task<PostModAction> Fetch(Guid id) => service.GetModActionAsync(id);

		/// <summary>
		/// Lists ModActions by Post or User IDs.
		/// </summary>
		/// <remarks>
		/// User ID gets ignored if a Post ID is provided.
		/// </remarks>
		/// <param name="postId">Get ModActions for specific Post.</param>
		/// <param name="userId">Get ModActions for specific User.</param>
		/// <response code="200">Returns list of ModActions.</response>
		/// <response code="204">No ModActions found.</response>
		[HttpGet("list"), AllowAnonymous, ProducesResponseType(typeof(IEnumerable<PostModActionDTO>), 200), ProducesResponseType(204)]
		public IActionResult List([FromQuery] Guid postId = default, [FromQuery] uint userId = default)
		{
			IEnumerable<PostModAction> modActions;

			if (postId != default)
			{
				modActions = service.GetPostModActions(postId).ToArray();
			}
			else if (userId is not 0)
			{
				modActions = service.GetPostModActions(userId).ToArray();
			}
			else
			{
				return BadRequest("Please use a search query (Post/User).");
			}

			return modActions?.Count() is null or 0
				? base.StatusCode(204)
				: base.StatusCode(200, modActions.Adapt<IEnumerable<PostModActionDTO>>());
		}

		/// <summary>
		/// Submits a new ModAction.
		/// </summary>
		/// <remarks>
		/// Usable only by Community Managers.
		/// </remarks>
		/// <param name="modAction">ModAction to submit</param>
		/// <response code="202">ModAction was successfully submitted.</response>
		[HttpPost, Authorize(Roles = ApiRoles.CM), ProducesResponseType(202)]
		public async Task<IActionResult> Submit([FromBody] PostModActionDTO modAction)
		{
			uint modId = uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
			await service.SubmitModActionAsync(modAction with { ModId = modId });
			return StatusCode(202);
		}

		/// <summary>
		/// Deletes an existing ModAction.
		/// </summary>
		/// <remarks>
		/// Usable only by Community Managers.
		/// </remarks>
		/// <param name="id">ID of ModAction to delete.</param>
		/// <response code="205">ModAction was flagged for deletion.</response>
		[HttpDelete("{id}"), Authorize(Roles = ApiRoles.CM), ProducesResponseType(205)]
		public async Task<IActionResult> Revert(Guid id)
		{
			await service.RevertModActionAsync(id);
			return StatusCode(205);
		}
	}
}
