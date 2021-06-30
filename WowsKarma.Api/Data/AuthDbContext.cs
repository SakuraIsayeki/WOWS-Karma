using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
			modelBuilder.HasDefaultSchema("Auth");
		}
	}
}
