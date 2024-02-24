using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Authentication.Cookie;

/// <summary>
/// Provides a cookie-based authentication implementation,
/// forwarding any authentication cookie to the Bearer token system.
/// </summary>
public sealed class ForwardCookieAuthenticationHandler : JwtAuthenticationHandler
{
	private readonly string _cookieName;
	
	public ForwardCookieAuthenticationHandler(IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, UserService userService, IConfiguration configuration)
		: base(options, logger, encoder, userService)
	{
		_cookieName = configuration[$"API:{Startup.ApiRegion.ToRegionString()}:CookieName"] 
			?? throw new($"Missing configuration value for API:{Startup.ApiRegion.ToRegionString()}:CookieName");
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (Request.Cookies.TryGetValue(_cookieName, out string? cookie))
		{
			Request.Headers.Authorization = new($"Bearer {cookie}");
		}

		return base.HandleAuthenticateAsync();
	}
	
}