using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;

namespace framework.jwt
{
	public class JwtService
	{
		private readonly JwtConfig _configuration = new();
		private readonly ILogger<JwtService> _logger;

		public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
		{
			configuration.Bind("Jwt", _configuration);
			_logger = logger;
		}

//		public string GenerateToken(ClaimsIdentity claimsIdentity, DateTime? expires = null) => GenerateToken(claimsIdentity, null, expires);

		public string GenerateToken(ClaimsIdentity claimsIdentity, string clientId, DateTime? expires = null)
		{
			if (!expires.HasValue)
			{
				expires = DateTime.UtcNow.AddDays(7);
			}

			return GenerateToken(claimsIdentity, expires.Value, GetClient(clientId), new() { { "type", "access" } });
		}

		public string GenerateRefreshToken(ClaimsIdentity claimsIdentity, DateTime? expires = null) => GenerateRefreshToken(claimsIdentity, null, expires);

		public string GenerateRefreshToken(ClaimsIdentity claimsIdentity, string clientId, DateTime? expires = null)
		{
			if (!expires.HasValue)
			{
				expires = DateTime.UtcNow.AddDays(7);
			}

			return GenerateToken(claimsIdentity, expires.Value, GetClient(clientId), new() { { "type", "refresh" } });
		}


		private static string GenerateToken(ClaimsIdentity claimsIdentity, DateTime expires, JwtConfigClient client, Dictionary<string, object> additionalClaims)
		{
			JwtSecurityTokenHandler tokenHandler = new();

			if (client.Secret?.Length < 16)
			{
				throw new SecurityException("The secret has to be atleast 16 characters long");
			}

			SecurityTokenDescriptor tokenDescriptor = new()
			{
				Subject = claimsIdentity,
				Expires = expires,
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(client.Secret)), SecurityAlgorithms.HmacSha256Signature),
				Claims = additionalClaims
			};

			if (string.IsNullOrEmpty(client.Audience))
			{
				tokenDescriptor.Audience = client.Audience;
			}

			return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
		}

		public ClaimsPrincipal ValidateToken(string token) => ValidateToken(token, string.Empty);
		public ClaimsPrincipal ValidateToken(string token, string clientId) => ValidateToken(token, GetClient(clientId));

		public ClaimsPrincipal ValidateRefreshToken(string token) => ValidateRefreshToken(token, string.Empty);
		public ClaimsPrincipal ValidateRefreshToken(string token, string clientId) => ValidateRefreshToken(token, GetClient(clientId));

		private ClaimsPrincipal ValidateToken(string token, JwtConfigClient client)
		{
			try
			{
				ValidateToken(token, client, out ClaimsPrincipal principal, out SecurityToken validatedToken);

				if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Claims.Any(c => c.Type is "type" && c.Value is "access"))
				{
					return principal;
				}
			}
			catch (SecurityTokenException ex)
			{
				_logger.LogInformation(ex, "Token validation falied");
				return null;
			}
			return null;
		}

		private ClaimsPrincipal ValidateRefreshToken(string token, JwtConfigClient client)
		{
			try
			{
				ValidateToken(token, client, out ClaimsPrincipal principal, out SecurityToken validatedToken);

				if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Claims.Any(c => c.Type is "type" && c.Value is "refresh"))
				{
					return principal;
				}
			}
			catch (SecurityTokenException ex)
			{
				_logger.LogInformation(ex, "Refresh token validation falied");
				return null;
			}
			return null;
		}


		private void ValidateToken(string token, JwtConfigClient client, out ClaimsPrincipal principal, out SecurityToken validatedToken)
		{
			principal = new JwtSecurityTokenHandler().ValidateToken(
				token, 
				new TokenValidationParameters()
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(client.Secret)),
					ClockSkew = TimeSpan.FromSeconds(_configuration.ClockSkew),
					ValidateLifetime = true,
					RequireExpirationTime = true,
					ValidateIssuer = false,
					ValidateAudience = client.Audience is not null,
					ValidAudience = client.Audience,

				}, out validatedToken);
		}

		private JwtConfigClient GetClient(string clientId)
		{
			if (_configuration.Clients.TryGetValue(clientId, out JwtConfigClient client))
			{
				return client;
			}

			throw new Exception($"Client with id {clientId} not found in Jwt config section");
		}
	}
}
