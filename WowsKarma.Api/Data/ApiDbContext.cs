using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;

namespace WowsKarma.Api.Data
{
	public class ApiDbContext : DbContext
	{
		public DbSet<Player> Players { get; set; }
		public DbSet<Post> Posts { get; set; }

		public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			#region Players

			modelBuilder.Entity<Player>()
				.HasMany(p => p.PostsReceived)
				.WithOne(p => p.Player);

			modelBuilder.Entity<Player>()
				.HasMany(p => p.PostsSent)
				.WithOne(p => p.Author);

			#endregion
		}
	}
}
