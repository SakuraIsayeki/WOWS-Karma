using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WowsKarma.Common;
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
		private readonly HttpClient httpClient;

		public ApiTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
			ISystemClock clock, IConfiguration configuration, JwtSecurityTokenHandler tokenHandler, IHttpClientFactory httpClientFactory) 
			: base(options, logger, encoder, clock)
		{
			CookieName ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:CookieName"];
			loginPath ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:Login"];
			websitePath ??= configuration[$"Api:{Utilities.CurrentRegion.ToRegionString()}:WebDomain"];
			this.tokenHandler = tokenHandler;
			httpClient = httpClientFactory.CreateClient();
		}
	
		protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			try
			{
				if (tokenHandler.ReadJwtToken(Request.Cookies[CookieName]) is JwtSecurityToken token)
				{
					using HttpRequestMessage request = new(HttpMethod.Head, "auth");
					request.Headers.Authorization = new("Bearer", Request.Cookies[CookieName]);
					using HttpResponseMessage response = await httpClient.SendAsync(request);

					if (!response.IsSuccessStatusCode)
					{
						Logger.LogInformation("API denied authentication token for Host {host}.", Request.Host.Host);
						return AuthenticateResult.NoResult();
					}

					ClaimsPrincipal principal = new(new ClaimsIdentity(token.Payload.Claims, AuthenticationScheme));

					Logger.LogInformation("Authenticated user {userId} from Host {host}.", principal.Identity.Name, Request.Host.Host);
					return AuthenticateResult.Success(new(principal, AuthenticationScheme));
				}
			}
			catch (ArgumentException)
			{
				Logger.LogInformation("Invalid token read from Host {host} while attempting to authenticate.", Request.Host.Host);
				return AuthenticateResult.NoResult();
			}
			catch (Exception e)
			{
				return AuthenticateResult.Fail(e.Message);
			}

			return AuthenticateResult.NoResult();
		}

		protected override Task HandleChallengeAsync(AuthenticationProperties properties)
		{
			properties.RedirectUri = $"{loginPath}?redirectUri={websitePath}/{properties.RedirectUri}";
			Response.Redirect(properties.RedirectUri);

			return Task.CompletedTask;
		}
	}
}
