using Microsoft.EntityFrameworkCore;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;
using Npgsql;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Utilities;

namespace WowsKarma.Api.Data;

public sealed class ApiDbContext : DbContext
{
	public DbSet<Clan> Clans { get; init; } = null!;
	public DbSet<ClanMember> ClanMembers { get; init; } = null!;
	public DbSet<PlatformBan> PlatformBans { get; init; } = null!;
	public DbSet<Player> Players { get; init; } = null!;
	public DbSet<Post> Posts { get; init; } = null!;
	public DbSet<PostModAction> PostModActions { get; init; } = null!;
	public DbSet<Replay> Replays { get; init; } = null!;

	#region Notifications
	public DbSet<NotificationBase> Notifications { get; init; } = null!;

	public DbSet<PlatformBanNotification> PlatformBanNotifications { get; init; } = null!;
	public DbSet<PostAddedNotification> PostAddedNotifications { get; init; } = null!;
	public DbSet<PostEditedNotification> PostEditedNotifications { get; init; } = null!;
	public DbSet<PostDeletedNotification> PostDeletedNotifications { get; init; } = null!;
	public DbSet<PostModEditedNotification> PostModEditedNotifications { get; init; } = null!;
	public DbSet<PostModDeletedNotification> PostModDeletedNotifications { get; init; } = null!;
	#endregion

	public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
	{
		
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		foreach (Type type in modelBuilder.Model.GetEntityTypes().Where(t => t.ClrType.ImplementsInterface(typeof(ITimestamped))).Select(t => t.ClrType))
		{
			modelBuilder.Entity(type)
				.Property<DateTimeOffset>(nameof(ITimestamped.CreatedAt))
					.ValueGeneratedOnAdd()
					.HasDefaultValueSql("NOW()");
		}

		#region Clans

		modelBuilder.HasPostgresEnum<ClanRole>();
		
		#endregion // Clans

		#region ClanMembers

		modelBuilder.Entity<ClanMember>()
			.HasKey(c => c.PlayerId);

		#endregion

		#region Notifications

		modelBuilder.HasPostgresEnum<NotificationType>();

		modelBuilder.Entity<NotificationBase>()
			.HasDiscriminator(n => n.Type)
				.HasValue<NotificationBase>(NotificationType.Unknown)
				.HasValue<PostAddedNotification>(NotificationType.PostAdded)
				.HasValue<PostEditedNotification>(NotificationType.PostEdited)
				.HasValue<PostDeletedNotification>(NotificationType.PostDeleted)
				.HasValue<PostModEditedNotification>(NotificationType.PostModEdited)
				.HasValue<PostModDeletedNotification>(NotificationType.PostModDeleted)
				.HasValue<PlatformBanNotification>(NotificationType.PlatformBan)
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
			.HasMany(p => p.PlatformBans)
			.WithOne(p => p.User)
			.HasForeignKey(p => p.UserId)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.Restrict);

		#endregion

		#region Posts

		modelBuilder.Entity<Post>()
			.HasOne(p => p.Replay)
			.WithOne(r => r.Post)
			.HasForeignKey<Post>(p => p.ReplayId)
			.IsRequired(false);

		#endregion	// Posts

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


public static class ApiDbContextExtensions
{
	public static NpgsqlDataSourceBuilder ConfigureApiDbDataSourceBuilder(this NpgsqlDataSourceBuilder dataSourceBuilder)
	{
		dataSourceBuilder
			.MapEnum<ModActionType>()
			.MapEnum<NotificationType>()
			.MapEnum<ClanRole>();
		
		dataSourceBuilder.EnableDynamicJson();

		return dataSourceBuilder;
	}
}