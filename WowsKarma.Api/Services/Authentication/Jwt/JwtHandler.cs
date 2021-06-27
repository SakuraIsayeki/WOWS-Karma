using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace framework.jwt
{
	public class JwtHandler : AuthenticationHandler<JwtOptions>
	{
		private readonly JwtService _jwtService;

		public JwtHandler(IOptionsMonitor<JwtOptions> options, JwtService jwtService, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
			: base(options, logger, encoder, clock)
		{
			_jwtService = jwtService;
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			string token = null;
			JwtResultContext messageReceivedContext = new(Context, Scheme, Options);

			if (messageReceivedContext.Result is not null)
			{
				return Task.FromResult(messageReceivedContext.Result);
			}

			string authorization = Request.Headers[HeaderNames.Authorization].FirstOrDefault();

			if (Request.Query.TryGetValue(HeaderNames.Authorization, out Microsoft.Extensions.Primitives.StringValues queryValues))
			{
				authorization = queryValues.First();
			}

			if (string.IsNullOrEmpty(authorization))
			{
				return Task.FromResult(AuthenticateResult.NoResult());
			}

			if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			{
				token = authorization["Bearer ".Length..].Trim();
			}
			else
			{
				token = authorization;
			}

			if (string.IsNullOrEmpty(token))
			{
				return Task.FromResult(AuthenticateResult.NoResult());
			}


			System.Security.Claims.ClaimsPrincipal principal = _jwtService.ValidateToken(token, Options.ClientId);
			if (principal == null)
			{
				return Task.FromResult(AuthenticateResult.Fail("Token validation unsuccessfull"));
			}

			JwtResultContext tokenValidatedContext = new JwtResultContext(Context, Scheme, Options);

			tokenValidatedContext.Principal = principal;
			if (tokenValidatedContext.Result != null)
			{
				return Task.FromResult(tokenValidatedContext.Result);
			}

			if (Options.SaveToken)
			{
				tokenValidatedContext.Properties.StoreTokens(new[]
				{
					new AuthenticationToken { Name = "access_token", Value = token }
				  });
			}

			tokenValidatedContext.Success();
			return Task.FromResult(tokenValidatedContext.Result);

		}

		protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
		{
			Response.Clear();
			Response.StatusCode = 403;
			return base.HandleForbiddenAsync(properties);
		}
	}
}
