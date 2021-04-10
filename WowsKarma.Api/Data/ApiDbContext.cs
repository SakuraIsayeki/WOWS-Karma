using Microsoft.EntityFrameworkCore;
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
				.WithOne(p => p.Player)
				.HasForeignKey(p => p.PlayerId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Player>()
				.HasMany(p => p.PostsSent)
				.WithOne(p => p.Author)
				.HasForeignKey(p => p.AuthorId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);


			modelBuilder.Entity<Player>()
				.Property(p => p.CreatedAt)
				.ValueGeneratedOnAdd()
				.HasDefaultValueSql("NOW()");

			#endregion

			#region Posts

			modelBuilder.Entity<Post>()
				.Property(p => p.CreatedAt)
				.ValueGeneratedOnAdd()
				.HasDefaultValueSql("NOW()");

			#endregion
		}
	}
}
