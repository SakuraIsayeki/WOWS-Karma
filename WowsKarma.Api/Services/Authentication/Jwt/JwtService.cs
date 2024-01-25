using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WowsKarma.Api.Services.Authentication.Jwt;

public sealed class JwtService
{
	internal JwtSecurityTokenHandler TokenHandler { get; private init; }

	private static IConfiguration _configuration = null!;
	private static SymmetricSecurityKey _authSigningKey = null!;

	public JwtService(IConfiguration configuration, JwtSecurityTokenHandler handler)
	{
		_configuration = configuration;
		TokenHandler = handler;

		_authSigningKey = new(Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? throw new("Missing JWT:Secret configuration value")));
	}

	public static JwtSecurityToken GenerateToken(IEnumerable<Claim> authClaims) => new(
		issuer: _configuration["JWT:ValidIssuer"],
		audience: _configuration["JWT:ValidAudience"],
		expires: DateTime.UtcNow.AddDays(8),
		claims: authClaims,
		signingCredentials: new(_authSigningKey, SecurityAlgorithms.HmacSha256));

	public ClaimsPrincipal? ValidateToken(string token)
	{
		TokenValidationParameters validationParameters = new()
		{
			IssuerSigningKey = _authSigningKey,
			ValidAudience = _configuration["JWT:ValidAudience"],
			ValidIssuer = _configuration["JWT:ValidIssuer"],
			ValidateLifetime = true,
			ValidateAudience = true,
			ValidateIssuer = true,
			ValidateIssuerSigningKey = true
		};

		try
		{
			return TokenHandler.ValidateToken(token, validationParameters, out _);
		}
		catch (SecurityTokenException)
		{
			return null;
		}
	}
}