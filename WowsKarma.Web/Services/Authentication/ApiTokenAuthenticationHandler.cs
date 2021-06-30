using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Common.Utilities;

namespace WowsKarma.Web.Services.Authentication
{
	public class ApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		public const string AuthenticationScheme = "ApiToken";

		public static string CookieName { get; private set; }

		private static string loginPath;
		private static string websitePath;

		private readonly JwtSecurityTokenHandler tokenHandler;

		public ApiTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
			ISystemClock clock, IConfiguration configuration, JwtSecurityTokenHandler tokenHandler) : base(options, logger, encoder, clock)
		{
			CookieName ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:CookieName"];
			loginPath ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:Login"];
			websitePath ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:WebDomain"];
			this.tokenHandler = tokenHandler;
		}
	
		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			try
			{
				if (tokenHandler.ReadJwtToken(Request.Cookies[CookieName]) is JwtSecurityToken token)
				{
					ClaimsPrincipal principal = new(new ClaimsIdentity(token.Payload.Claims, AuthenticationScheme));

					Logger.LogInformation("Authenticated user {userId} from Host {host}.", principal.Identity.Name, Request.Host.Host);
					return Task.FromResult(AuthenticateResult.Success(new(principal, AuthenticationScheme)));
				}
			}
			catch (ArgumentException)
			{
				Logger.LogInformation("Invalid token read from Host {host} while attempting to authenticate.", Request.Host.Host);
				return Task.FromResult(AuthenticateResult.NoResult());
			}

			return Task.FromResult(AuthenticateResult.NoResult());
		}

		protected override Task HandleChallengeAsync(AuthenticationProperties properties)
		{
			properties.RedirectUri = $"{loginPath}?redirectUri={websitePath}/{properties.RedirectUri}";
			Response.Redirect(properties.RedirectUri);

			return Task.CompletedTask;
		}
	}
}
