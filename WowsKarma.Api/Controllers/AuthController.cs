using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common;
using static WowsKarma.Common.Utilities;



namespace WowsKarma.Api.Controllers
{
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

		[HttpHead, Authorize]
		public IActionResult ValidateAuth() => StatusCode(200);

		[HttpGet("login")]
		public IActionResult Login() => wargamingAuthService.RedirectToLogin(Startup.ApiRegion, Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.FirstOrDefault()));

		[HttpGet("wg-callback")]
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
					Domain = config[$"Api:{Startup.ApiRegion.ToRegionString()}:DomainName"],
					HttpOnly = true,
					IsEssential = true,
					Expires = DateTimeOffset.UtcNow.AddDays(7)
				});

			return Request.Query["redirectUri"].FirstOrDefault() is string redirectUri 
				? Redirect(redirectUri)
				: StatusCode(200);
		}

		[HttpPost("renew-seed"), Authorize]
		public async Task<IActionResult> RenewSeed() 
		{
			await userService.RenewSeedTokenAsync(uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
			return StatusCode(200);
		}

		[HttpGet("refresh-token"), Authorize]
		public async Task<IActionResult> RefreshToken()
		{
			JwtSecurityToken token = await userService.CreateTokenAsync(new(User.Claims));
			return StatusCode(200, jwtService.TokenHandler.WriteToken(token));
		}
	}
}
