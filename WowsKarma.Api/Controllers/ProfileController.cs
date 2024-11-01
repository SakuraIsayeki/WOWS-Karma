﻿using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WowsKarma.Api.Infrastructure.Attributes;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Common;

namespace WowsKarma.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ProfileController : ControllerBase
{
	private readonly PlayerService _playerService;
	private readonly UserService _userService;

	public ProfileController(PlayerService playerService, UserService userService)
	{
		_playerService = playerService;
		_userService = userService;
	}

	/// <summary>
	/// Fetches a player's profile flags for given ID.
	/// This includes Platform Bans, and Opt-out statuses.
	/// </summary>
	/// <param name="id">Player ID to fetch profile flags from.</param>
	/// <response code="200">Returns player profile flags for given ID.</response>
	/// <response code="404">No player Profile was found.</response>
	[HttpGet("{id}")]
	public async Task<ActionResult<UserProfileFlagsDTO>> GetProfileFlagsAsync(uint id) => await _playerService.GetPlayerAsync(id, true) is { } player
		? Ok(player.Adapt<UserProfileFlagsDTO>() with
			{
				PostsBanned = player.IsBanned(),
				ProfileRoles = (await _userService.GetUserAsync(id))?.Roles.Select(r => r.Id) ?? []
			})
		: NotFound();

	/// <summary>
	/// Updates user-settable profile values.
	/// </summary>
	/// <param name="flags">
	/// Updated profile values to set on profile.
	/// Note: Platform Ban state cannot be edited through this endpoint.
	/// </param>
	/// <response code="200">Profile flags were successfuly updated.</response>
	/// <response code="403">User cannot update a profile other than their own.</response>
	/// <response code="404">User profile was not found.</response>
	/// <response code="423">A cooldown is currently in effect for one of the values edited.</response>
	[HttpPut, Authorize(RequireNoPlatformBans), ETag(false)]
	public async Task<ActionResult> UpdateProfileFlagsAsync([FromBody] UserProfileFlagsDTO flags)
	{
		try
		{
			if (flags.Id != User.ToAccountListing()!.Id && !User.IsInRole(ApiRoles.Administrator))
			{
				ModelState.AddModelError(nameof(flags.Id), "User can only update their own profile.");
				return BadRequest(ModelState);
			}

			await _playerService.UpdateProfileFlagsAsync(flags);
			return NoContent();
		}
		catch (CooldownException e)
		{
			ModelState.TryAddModelException(nameof(flags), e);
			return StatusCode(StatusCodes.Status423Locked, ModelState);
		}
		catch (ArgumentException)
		{
			return NotFound();
		}
	}
}