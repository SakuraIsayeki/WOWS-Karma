using Microsoft.EntityFrameworkCore;
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
	}
}
