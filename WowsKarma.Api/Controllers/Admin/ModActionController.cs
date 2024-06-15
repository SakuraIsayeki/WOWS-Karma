using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Posts;
using WowsKarma.Common;

namespace WowsKarma.Api.Controllers.Admin;


[ApiController, Route("api/mod/action"), Authorize(Roles = ApiRoles.CM)]
public sealed class ModActionController : ControllerBase
{
	private readonly ModService _service;

	public ModActionController(ModService service)
	{
		_service = service;
	}

	/// <summary>
	/// Fetches ModAction by ID.
	/// </summary>
	/// <param name="id">ID of ModAction to fetch.</param>
	[HttpGet("{id:guid}"), AllowAnonymous]
	public async Task<PostModActionDTO> Fetch(Guid id)
	{
		using PostModAction? pma = await _service.GetModActionAsync(id);
		return pma.Adapt<PostModActionDTO>();
	}

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
		PostModAction[] modActions;

		if (postId != default)
		{
			modActions = [.. _service.GetPostModActions(postId)];
		}
		else if (userId is not 0)
		{
			modActions = [.. _service.GetPostModActions(userId)];
		}
		else
		{
			return BadRequest("Please use a search query (Post/User).");
		}

		return modActions is []
			? NoContent()
			: Ok(modActions.Adapt<IEnumerable<PostModActionDTO>>());
	}

	/// <summary>
	/// Submits a new ModAction.
	/// </summary>
	/// <remarks>
	/// Usable only by Community Managers.
	/// </remarks>
	/// <param name="modAction">ModAction to submit</param>
	/// <param name="postService">(DI)</param>
	/// <response code="202">ModAction was successfully submitted.</response>
	[HttpPost, Authorize(Roles = ApiRoles.CM), ProducesResponseType(202)]
	public async Task<IActionResult> Submit([FromBody] PostModActionDTO modAction,
		[FromServices] PostService postService)
	{
		using Post post = postService.GetPost(modAction.PostId) ?? throw new InvalidOperationException("Post ID is invalid.");
		uint modId = uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new BadHttpRequestException("Missing NameIdentifier claim."));

		if ((post.AuthorId == modId || post.PlayerId == modId) && !User.IsInRole(ApiRoles.Administrator))
		{
			return StatusCode(403, $"CMs cannot act on Posts with relation to self. This restriction is lifted for users with {ApiRoles.Administrator} role.");
		}

		await _service.SubmitPostModActionAsync(modAction with { ModId = modId });
		return Accepted();
	}

	/// <summary>
	/// Reverts an existing ModAction.
	/// </summary>
	/// <remarks>
	/// Usable only by Community Managers and Administrators.
	/// </remarks>
	/// <param name="id">ID of ModAction to delete.</param>
	/// <response code="205">ModAction was flagged for deletion.</response>
	[HttpDelete("{id:guid}"), Authorize(Roles = $"{ApiRoles.Administrator},{ApiRoles.CM}"), ProducesResponseType(205)]
	public async Task<IActionResult> Revert(Guid id)
	{
		await _service.RevertModActionAsync(id);
		return StatusCode(205);
	}
}
