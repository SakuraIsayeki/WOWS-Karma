using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;
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

			WargamingIdentity identity = WargamingIdentity.FromUri(new Uri(Request.Query["openid.identity"].FirstOrDefault()));
			AccountListingDTO accountListing = identity.GetAccountListing();

			identity.AddClaims(await userService.GetUserClaimsAsync(accountListing.Id));
			identity.AddClaim(new("seed", (await userService.GetUserSeedTokenAsync(accountListing.Id)).ToString()));

			JwtSecurityToken token = JwtService.GenerateToken(identity.Claims.ToArray());

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

		[HttpGet("validate"), Authorize]
		public IActionResult ValidateAuth() => StatusCode(200);
	}
}
