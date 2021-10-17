using Microsoft.EntityFrameworkCore;
using Npgsql;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Common.Models;

namespace WowsKarma.Api.Data
{
	public class ApiDbContext : DbContext
	{
		public DbSet<Player> Players { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<PostModAction> PostModActions { get; set; }

		#region Notifications
		public DbSet<NotificationBase> Notifications { get; set; }

		public DbSet<PostModDeletedNotification> PostModDeletedNotifications { get; set; }
		#endregion


		static ApiDbContext()
		{
			NpgsqlConnection.GlobalTypeMapper.MapEnum<ModActionType>();
			NpgsqlConnection.GlobalTypeMapper.MapEnum<NotificationType>();
		}

		public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			#region Notifications

			modelBuilder.HasPostgresEnum<NotificationType>();

			modelBuilder.Entity<NotificationBase>()
				.HasDiscriminator(n => n.Type)
					.HasValue<NotificationBase>(NotificationType.Unknown)
					.HasValue<PostModDeletedNotification>(NotificationType.PostModDeleted)
					.IsComplete(false);





			#endregion    // Notifications

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

			#region	PostModActions

			modelBuilder.HasPostgresEnum<ModActionType>();

			modelBuilder.Entity<PostModAction>()
				.HasOne(pma => pma.Mod)
				.WithMany()
				.HasForeignKey(pma => pma.ModId);

			modelBuilder.Entity<PostModAction>()
				.HasOne(pma => pma.Post)
				.WithMany()
				.HasForeignKey(pma => pma.PostId);

			#endregion
		}
	}
}
