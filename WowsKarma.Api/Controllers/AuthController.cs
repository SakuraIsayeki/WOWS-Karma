using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WowsKarma.Api.Infrastructure.Attributes;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common;

namespace WowsKarma.Api.Controllers;

/// <summary>
/// Provides API Authentication endpoints.
/// </summary>
[ApiController, Route("api/[controller]"), ETag(false)]
public sealed class AuthController : ControllerBase
{
	private readonly IConfiguration _config;
	private readonly UserService _userService;
	private readonly WargamingAuthService _wargamingAuthService;
	private readonly JwtService _jwtService;

	
	public AuthController(IConfiguration config, UserService userService, WargamingAuthService wargamingAuthService, JwtService jwtService)
	{
		_config = config;
		_userService = userService;
		_wargamingAuthService = wargamingAuthService;
		_jwtService = jwtService;
	}

	/// <summary>
	/// Verifies current request's Authentication to API.
	/// </summary>
	/// <response code="200">Authentication successful.</response>
	/// <response code="401">Authentication failed.</response>
	[HttpHead, Authorize, ProducesResponseType(200), ProducesResponseType(401)]
	public IActionResult ValidateAuth() => StatusCode(200);

	/// <summary>
	/// Provides redirection to Wargaming OpenID Authentication.
	/// </summary>
	/// <response code="302">Redirection to Wargaming Auth</response>
	[HttpGet("login"), ProducesResponseType(302)]
	public IActionResult Login() => WargamingAuthService.RedirectToLogin(Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.FirstOrDefault()));

	/// <summary>
	/// Provides a callback endpoint for Wargaming OpenID results, and stores all authentication information to relevant cookies.
	/// </summary>
	/// <response code="302">Redirection after successful authentication (if redirectUri is set).</response>
	/// <response code="200">Authentication successful.</response>
	/// <response code="403">Invalid callback request.</response>
	[HttpGet("wg-callback"), ProducesResponseType(302), ProducesResponseType(200), ProducesResponseType(403)]
	public async Task<IActionResult> WgAuthCallback()
	{
		bool valid = await _wargamingAuthService.VerifyIdentity(Request);

		if (!valid)
		{
			return StatusCode(403);
		}

		JwtSecurityToken token = await _userService.CreateTokenAsync(WargamingIdentity.FromUri(new(Request.Query["openid.identity"].FirstOrDefault()
			?? throw new BadHttpRequestException("Missing OpenID identity"))));

		Response.Cookies.Append(
			_config[$"Api:{Startup.ApiRegion.ToRegionString()}:CookieName"] ?? throw new ApplicationException("Missing Api:{region}:CookieName in configuration."),
			_jwtService.TokenHandler.WriteToken(token),
			new()
			{
				Domain = _config[$"Api:{Startup.ApiRegion.ToRegionString()}:CookieDomain"],
				HttpOnly = false,
				IsEssential = true,
#if RELEASE
				Secure = true,
#endif
				Expires = DateTime.UtcNow.AddDays(7)
			});

		return Request.Query["redirectUri"].FirstOrDefault() is { } redirectUri
			? Redirect(redirectUri)
			: Ok();
	}

	/// <summary>
	/// Renews Seed-Token, invalidating all previously issued JWTs.
	/// </summary>
	/// <response code="200">Seed Token successfully reset.</response>
	/// <response code="401">Authentication failed.</response>
	[HttpPost("renew-seed"), Authorize, ProducesResponseType(200), ProducesResponseType(401)]
	public async Task<IActionResult> RenewSeed()
	{
		await _userService.RenewSeedTokenAsync(uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new BadHttpRequestException("Missing NameIdentifier claim.")));
		return Ok();
	}

	/// <summary>
	/// Issues a new JWT with information mirrored to current token.
	/// </summary>
	/// <response code="200">Token successfully refreshed.</response>
	/// <response code="401">Authentication failed.</response>
	[HttpGet("refresh-token"), Authorize, ProducesResponseType(typeof(string), 200), ProducesResponseType(401)]
	public async Task<IActionResult> RefreshToken()
	{
		JwtSecurityToken token = await _userService.CreateTokenAsync(new(User.Claims));
		return StatusCode(200, _jwtService.TokenHandler.WriteToken(token));
	}
}
