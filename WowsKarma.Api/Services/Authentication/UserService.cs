using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Auth;
using WowsKarma.Api.Hubs;
using WowsKarma.Api.Services.Authentication.Jwt;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common.Hubs;

namespace WowsKarma.Api.Services.Authentication;

/// <summary>
/// Provides a service to fetch and update API users.
/// </summary>
public sealed class UserService
{
	private const string SeedTokenClaimType = "seed";
	private readonly AuthDbContext _context;
	private readonly IHubContext<AuthHub, IAuthHubPush> _hubContext;

	public UserService(AuthDbContext context, IHubContext<AuthHub, IAuthHubPush> hubContext)
	{
		_context = context;
		_hubContext = hubContext;
	}

	/// <summary>
	/// Gets a user by their ID.
	/// </summary>
	/// <param name="id">The user's ID.</param>
	/// <returns>The user, or <see langword="null"/> if not found.</returns>
	public async Task<User?> GetUserAsync(uint id) => await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);

	/// <summary>
	/// Gets a user's claims
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public async Task<IEnumerable<Claim>> GetUserClaimsAsync(uint id) => await GetUserAsync(id) is { Roles: [..] roles }
		? from role in roles select new Claim(ClaimTypes.Role, role.InternalName)
		: [];

	public async Task<Guid> GetUserSeedTokenAsync(uint id)
	{
		if (await _context.Users.FindAsync(id) is not { } user)
		{
			user = new()
			{
				Id = id,
				SeedToken = Guid.NewGuid()
			};

			await _context.Users.AddAsync(user);
		}

		user.LastTokenRequested = DateTimeOffset.UtcNow;
		await _context.SaveChangesAsync();
		return user.SeedToken;
	}

	public async Task<bool> ValidateUserSeedTokenAsync(uint id, Guid seedToken) => await _context.Users.FindAsync(id) is { SeedToken: var st } && st == seedToken;

	public async Task RenewSeedTokenAsync(uint id)
	{
		if (await GetUserAsync(id) is { } user)
		{
			user.SeedToken = Guid.NewGuid();
			await _context.SaveChangesAsync();

			await _hubContext.Clients.All.SeedTokenInvalidated(user.Id);
		}
	}

	public async Task<JwtSecurityToken> CreateTokenAsync(WargamingIdentity identity)
	{
		(uint id, _) = identity.GetAccountListing();

		if (identity.Claims.Where(c => c.Type is ClaimTypes.Role).ToArray() is { Length: not 0 } claims)
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
