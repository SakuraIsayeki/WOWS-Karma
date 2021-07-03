using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Auth;

namespace WowsKarma.Api.Services
{
	public class UserService
	{
		private readonly AuthDbContext context;

		public UserService(IDbContextFactory<AuthDbContext> dbContextFactory)
		{
			context = dbContextFactory.CreateDbContext();
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
	}
}
