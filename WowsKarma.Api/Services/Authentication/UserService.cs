using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Auth;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Services.Authentication
{
	public class UserService
	{
		private const string SeedTokenClaimType = "seed";
		private readonly AuthDbContext context;

		public UserService(AuthDbContext context)
		{
			this.context = context;
		}

		public Task<User> GetUserAsync(uint id) => context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);

		public async Task<IEnumerable<Claim>> GetUserClaimsAsync(uint id) => await GetUserAsync(id) is User user
			? (from role in user.Roles select new Claim(ClaimTypes.Role, role.InternalName))
			: Enumerable.Empty<Claim>();

		public async Task<Guid> GetUserSeedTokenAsync(uint id)
		{
			if (await GetUserAsync(id) is not User user)
			{
				user = new()
				{
					Id = id,
					SeedToken = Guid.NewGuid()
				};

				await context.Users.AddAsync(user);
			}

			user.LastTokenRequested = DateTime.UtcNow;
			await context.SaveChangesAsync();
			return user.SeedToken;
		}

		public async Task<bool> ValidateUserSeedTokenAsync(uint id, Guid seedToken) => await GetUserAsync(id) is User user && user.SeedToken == seedToken;

		public async Task RenewSeedTokenAsync(uint id)
		{
			if (await GetUserAsync(id) is User user)
			{
				user.SeedToken = Guid.NewGuid();
			}

			await context.SaveChangesAsync();
		}

		public async Task<JwtSecurityToken> CreateTokenAsync(WargamingIdentity identity)
		{
			AccountListingDTO accountListing = identity.GetAccountListing();

			if (identity.Claims.Where(c => c.Type is ClaimTypes.Role).ToArray() is IEnumerable<Claim> claims && claims.Any())
			{
				foreach (Claim c in claims)
				{
					identity.RemoveClaim(c);
				}
			}

			identity.AddClaims(await GetUserClaimsAsync(accountListing.Id));


			if (identity.Claims.FirstOrDefault(c => c.Type is SeedTokenClaimType) is Claim seedClaim)
			{
				identity.RemoveClaim(seedClaim);
			}

			if (identity.Claims.FirstOrDefault(c => c.Type is "aud") is Claim audClaim)
			{
				identity.RemoveClaim(audClaim);
			}

			identity.AddClaim(new(SeedTokenClaimType, (await GetUserSeedTokenAsync(accountListing.Id)).ToString()));

			return JwtService.GenerateToken(identity.Claims);
		}
	}
}
