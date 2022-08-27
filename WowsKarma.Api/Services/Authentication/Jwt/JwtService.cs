using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace WowsKarma.Api.Services.Authentication.Jwt;

public class JwtService
{
	internal JwtSecurityTokenHandler TokenHandler { get; private init; }

	private static IConfiguration configuration;
	private static SymmetricSecurityKey authSigningKey;

	public JwtService(IConfiguration configuration, JwtSecurityTokenHandler handler)
	{
		JwtService.configuration ??= configuration;
		TokenHandler = handler;

		authSigningKey = new(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
	}

	public static JwtSecurityToken GenerateToken(IEnumerable<Claim> authClaims) => new(
		issuer: configuration["JWT:ValidIssuer"],
		audience: configuration["JWT:ValidAudience"],
		expires: DateTime.UtcNow.AddDays(8),
		claims: authClaims,
		signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

	public ClaimsPrincipal ValidateToken(string token)
	{
		TokenValidationParameters validationParameters = new()
		{
			IssuerSigningKey = authSigningKey,
			ValidAudience = configuration["JWT:ValidAudience"],
			ValidIssuer = configuration["JWT:ValidIssuer"],
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