using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common;


namespace WowsKarma.Api.Controllers;


/// <summary>
/// Provides API Authentication endpoints.
/// </summary>
[ApiController, Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IConfiguration config;
	private readonly UserService userService;
	private readonly WargamingAuthService wargamingAuthService;
	private readonly JwtService jwtService;

	public AuthController(IConfiguration config, UserService userService, WargamingAuthService wargamingAuthService, JwtService jwtService)
	{
		this.config = config;
		this.userService = userService;
		this.wargamingAuthService = wargamingAuthService;
		this.jwtService = jwtService;
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
		bool valid = await wargamingAuthService.VerifyIdentity(Request);

		if (!valid)
		{
			return StatusCode(403);
		}

		JwtSecurityToken token = await userService.CreateTokenAsync(WargamingIdentity.FromUri(new Uri(Request.Query["openid.identity"].FirstOrDefault())));

		Response.Cookies.Append(
			config[$"Api:{Startup.ApiRegion.ToRegionString()}:CookieName"],
			jwtService.TokenHandler.WriteToken(token),
			new()
			{
				Domain = config[$"Api:{Startup.ApiRegion.ToRegionString()}:CookieDomain"],
				HttpOnly = true,
				IsEssential = true,
				Expires = DateTimeOffset.UtcNow.AddDays(7)
			});

		return Request.Query["redirectUri"].FirstOrDefault() is string redirectUri
			? Redirect(redirectUri)
			: StatusCode(200);
	}

	/// <summary>
	/// Renews Seed-Token, invalidating all previously issued JWTs.
	/// </summary>
	/// <response code="200">Seed Token successfully reset.</response>
	/// <response code="401">Authentication failed.</response>
	[HttpPost("renew-seed"), Authorize, ProducesResponseType(200), ProducesResponseType(401)]
	public async Task<IActionResult> RenewSeed()
	{
		await userService.RenewSeedTokenAsync(uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
		return StatusCode(200);
	}

	/// <summary>
	/// Issues a new JWT with information mirrored to current token.
	/// </summary>
	/// <response code="200">Token successfully refreshed.</response>
	/// <response code="401">Authentication failed.</response>
	[HttpGet("refresh-token"), Authorize, ProducesResponseType(typeof(string), 200), ProducesResponseType(401)]
	public async Task<IActionResult> RefreshToken()
	{
		JwtSecurityToken token = await userService.CreateTokenAsync(new(User.Claims));
		return StatusCode(200, jwtService.TokenHandler.WriteToken(token));
	}
}
