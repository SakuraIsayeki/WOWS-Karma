﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Auth;
using WowsKarma.Api.Hubs;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common;
using WowsKarma.Common.Hubs;

namespace WowsKarma.Api.Services.Authentication;

public class UserService
{
	private const string SeedTokenClaimType = "seed";
	private readonly AuthDbContext context;
	private readonly IHubContext<AuthHub, IAuthHubPush> _hubContext;

	public UserService(AuthDbContext context, IHubContext<AuthHub, IAuthHubPush> hubContext)
	{
		this.context = context;
		_hubContext = hubContext;
	}

	public Task<User> GetUserAsync(uint id) => context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);

	public async Task<IEnumerable<Claim>> GetUserClaimsAsync(uint id) => await GetUserAsync(id) is { } user
		? from role in user.Roles select new Claim(ClaimTypes.Role, role.InternalName)
		: Enumerable.Empty<Claim>();

	public async Task<Guid> GetUserSeedTokenAsync(uint id)
	{
		if (await context.Users.FindAsync(id) is not { } user)
		{
			user = new()
			{
				Id = id,
				SeedToken = Guid.NewGuid()
			};

			await context.Users.AddAsync(user);
		}

		user.LastTokenRequested = DateTimeOffset.UtcNow;
		await context.SaveChangesAsync();
		return user.SeedToken;
	}

	public async Task<bool> ValidateUserSeedTokenAsync(uint id, Guid seedToken) => await context.Users.FindAsync(id) is { } user && user.SeedToken == seedToken;

	public async Task RenewSeedTokenAsync(uint id)
	{
		if (await GetUserAsync(id) is { } user)
		{
			user.SeedToken = Guid.NewGuid();
			await context.SaveChangesAsync();

			await _hubContext.Clients.All.SeedTokenInvalidated(user.Id);
		}
	}

	public async Task<JwtSecurityToken> CreateTokenAsync(WargamingIdentity identity)
	{
		(uint id, _) = identity.GetAccountListing();

		if (identity.Claims.Where(c => c.Type is ClaimTypes.Role).ToArray() is { Length: > 0 } claims)
		{
			foreach (Claim c in claims)
			{
				identity.RemoveClaim(c);
			}
		}

		identity.AddClaims(await GetUserClaimsAsync(id));


		if (identity.Claims.FirstOrDefault(c => c.Type is SeedTokenClaimType) is { } seedClaim)
		{
			identity.RemoveClaim(seedClaim);
		}

		if (identity.Claims.FirstOrDefault(c => c.Type is "aud") is { } audClaim)
		{
			identity.RemoveClaim(audClaim);
		}

		identity.AddClaim(new(SeedTokenClaimType, (await GetUserSeedTokenAsync(id)).ToString()));

		return JwtService.GenerateToken(identity.Claims);
	}
}
