using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WowsKarma.Api.Services.Authentication.Jwt;

public class JwtAuthenticationHandler : JwtBearerHandler
{
	private readonly UserService _userService;

	public JwtAuthenticationHandler(IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, UserService userService)
		: base(options, logger, encoder)
	{
		_userService = userService;
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		AuthenticateResult baseResult = await base.HandleAuthenticateAsync();

		if (!baseResult.Succeeded)
		{
			return baseResult;
		}

		bool isValid = false;
		Exception? failure = null;

		try
		{
			if (baseResult.Principal.FindFirstValue("seed") is { } seed
				&& uint.TryParse(baseResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier), out uint id)
				&& await _userService.ValidateUserSeedTokenAsync(id, new(seed)))
			{
				isValid = true;
			}
		}
		catch (Exception e)
		{
			isValid = false;
			failure = e;
		}

		return isValid
			? baseResult
			: failure is null
				? AuthenticateResult.NoResult()
				: AuthenticateResult.Fail(failure.Message);
	}
}