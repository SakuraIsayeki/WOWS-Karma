using Microsoft.EntityFrameworkCore;
using WowsKarma.Api.Data.Models.Auth;

namespace WowsKarma.Api.Data
{
	public class AuthDbContext : DbContext
	{
		public DbSet<User> Users { get; init; }
		public DbSet<Role> Roles { get; init; }

		public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {	}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("auth");
		}
	}
}
