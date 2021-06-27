using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WowsKarma.Api.Services.Authentication.Jwt
{
	public class JwtAuthService
	{
		private static IConfiguration configuration;
		private static SymmetricSecurityKey authSigningKey;

		public JwtAuthService(IConfiguration configuration)
		{
			JwtAuthService.configuration ??= configuration;
			authSigningKey = new(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
		}

		public static JwtSecurityToken GenerateToken(Claim[] authClaims) => new(
				issuer: configuration["JWT:ValidIssuer"],
				audience: configuration["JWT:ValidAudience"],
				expires: DateTime.Now.AddDays(7),
				claims: authClaims,
				signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
	}
}
