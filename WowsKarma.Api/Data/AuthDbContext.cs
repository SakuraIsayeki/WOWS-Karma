using Microsoft.EntityFrameworkCore;
using WowsKarma.Api.Data.Models.Auth;
using WowsKarma.Common;

namespace WowsKarma.Api.Data
{
	public class AuthDbContext : DbContext
	{
		public DbSet<User> Users { get; init; }
		public DbSet<Role> Roles { get; init; }

		public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("auth");

			modelBuilder.Entity<Role>()
				.HasData(
					new Role { Id = 1, InternalName = ApiRoles.Administrator, DisplayName = "Administrator" },
					new Role { Id = 2, InternalName = ApiRoles.CM, DisplayName = "Community Manager" }
				);
		}
	}
}
