using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Services;
using WowsKarma.Common;

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
	}
}
