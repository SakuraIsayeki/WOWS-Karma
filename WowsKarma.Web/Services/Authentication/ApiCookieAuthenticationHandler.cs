using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
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
	public class ApiCookieAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		public const string AuthenticationScheme = "ApiCookie";

		public static string CookieName { get; private set; }

		private static string loginPath;
		private static string websitePath;

		public ApiCookieAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, 
			UrlEncoder encoder, ISystemClock clock, IConfiguration configuration) : base(options, logger, encoder, clock)
		{
			CookieName ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:CookieName"];
			loginPath ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:Login"];
			websitePath ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:WebDomain"];
		}
	
		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (Request.Cookies[CookieName] is string cookie)
			{
				IEnumerable<UserClaimDTO> claims = JsonSerializer.Deserialize<IEnumerable<UserClaimDTO>>(cookie, CookieSerializerOptions);

				if (claims.Any(c => c.Key is "token"))
				{
					ClaimsPrincipal principal = new(new ClaimsIdentity(claims.Select(c => new Claim(c.Key, c.Value)), AuthenticationScheme));

					Logger.LogInformation("Authenticated user {userId} from Host {host}.", principal.Identity.Name, Request.Host.Host);
					return Task.FromResult(AuthenticateResult.Success(new(principal, AuthenticationScheme)));
				}
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
