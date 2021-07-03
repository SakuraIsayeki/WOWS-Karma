using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Services;
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
		private readonly JwtAuthService jwtService;
		private readonly JwtSecurityTokenHandler tokenHandler;

		public AuthController(IConfiguration config, UserService userService, WargamingAuthService wargamingAuthService, 
			JwtAuthService jwtService, JwtSecurityTokenHandler tokenHandler)
		{
			this.config = config;
			this.userService = userService;
			this.wargamingAuthService = wargamingAuthService;
			this.jwtService = jwtService;
			this.tokenHandler = tokenHandler;
		}

		[HttpGet("test-auth")]
		public IActionResult TestAuth() => StatusCode(200, HttpContext.User.Claims.ToDictionary(kv => kv.Type, kv => kv.Value.FirstOrDefault()));

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
			identity.AddClaims(await userService.GetUserClaimsAsync(identity.GetAccountIdentification().Id));
			
			JwtSecurityToken token = JwtAuthService.GenerateToken(identity.Claims.ToArray());

			Response.Cookies.Append(
				config[$"Api:{Startup.ApiRegion.ToRegionString()}:CookieName"],
				tokenHandler.WriteToken(token),
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
	}
}
