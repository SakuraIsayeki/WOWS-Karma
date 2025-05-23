﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Common.Models;

#nullable disable

namespace WowsKarma.Api.Migrations
{
    [DbContext(typeof(ApiDbContext))]
    [Migration("20240901093435_AddCSTicketId")]
    partial class AddCSTicketId
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "clan_role", new[] { "unknown", "commander", "executive_officer", "recruitment_officer", "commissioned_officer", "officer", "private" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "mod_action_type", new[] { "deletion", "update" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "notification_type", new[] { "unknown", "other", "post_added", "post_edited", "post_deleted", "post_mod_edited", "post_mod_deleted", "platform_ban" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Clan", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDisbanded")
                        .HasColumnType("boolean");

                    b.Property<long>("LeagueColor")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("MembersUpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.ClanMember", b =>
                {
                    b.Property<long>("PlayerId")
                        .HasColumnType("bigint");

                    b.Property<long>("ClanId")
                        .HasColumnType("bigint");

                    b.Property<DateOnly>("JoinedAt")
                        .HasColumnType("date");

                    b.Property<DateOnly?>("LeftAt")
                        .HasColumnType("date");

                    b.Property<ClanRole>("Role")
                        .HasColumnType("clan_role");

                    b.HasKey("PlayerId");

                    b.HasIndex("ClanId");

                    b.ToTable("ClanMembers");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.NotificationBase", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset?>("AcknowledgedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("EmittedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<NotificationType>("Type")
                        .HasColumnType("notification_type");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("Notifications");

                    b.HasDiscriminator<NotificationType>("Type").IsComplete(false).HasValue(NotificationType.Unknown);

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.PlatformBan", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("BannedUntil")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<long>("ModId")
                        .HasColumnType("bigint");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Reverted")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ModId");

                    b.HasIndex("UserId");

                    b.ToTable("PlatformBans");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Player", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<int>("CourtesyRating")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<int>("GameKarma")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("LastBattleTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("OptOutChanged")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("OptedOut")
                        .HasColumnType("boolean");

                    b.Property<int>("PerformanceRating")
                        .HasColumnType("integer");

                    b.Property<bool>("PostsBanned")
                        .HasColumnType("boolean");

                    b.Property<int>("SiteKarma")
                        .HasColumnType("integer");

                    b.Property<int>("TeamplayRating")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("WgAccountCreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("WgHidden")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("AuthorId")
                        .HasColumnType("bigint");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<int?>("CustomerSupportTicketId")
                        .HasColumnType("integer");

                    b.Property<int>("Flairs")
                        .HasColumnType("integer");

                    b.Property<bool>("ModLocked")
                        .HasColumnType("boolean");

                    b.Property<bool>("NegativeKarmaAble")
                        .HasColumnType("boolean");

                    b.Property<long>("PlayerId")
                        .HasColumnType("bigint");

                    b.Property<bool>("ReadOnly")
                        .HasColumnType("boolean");

                    b.Property<Guid?>("ReplayId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("PlayerId");

                    b.HasIndex("ReplayId")
                        .IsUnique();

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.PostModAction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<ModActionType>("ActionType")
                        .HasColumnType("mod_action_type");

                    b.Property<long>("ModId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ModId");

                    b.HasIndex("PostId");

                    b.ToTable("PostModActions");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Replays.Replay", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<JsonDocument>("ArenaInfo")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("BlobName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<IEnumerable<ReplayChatMessage>>("ChatMessages")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<bool>("MinimapRendered")
                        .HasColumnType("boolean");

                    b.Property<IEnumerable<ReplayPlayer>>("Players")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Replays");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PlatformBanNotification", b =>
                {
                    b.HasBaseType("WowsKarma.Api.Data.Models.Notifications.NotificationBase");

                    b.Property<Guid>("BanId")
                        .HasColumnType("uuid");

                    b.HasIndex("BanId");

                    b.HasDiscriminator().HasValue(NotificationType.PlatformBan);
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostAddedNotification", b =>
                {
                    b.HasBaseType("WowsKarma.Api.Data.Models.Notifications.NotificationBase");

                    b.Property<Guid>("PostId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("uuid");

                    b.HasIndex("PostId");

                    b.HasDiscriminator().HasValue(NotificationType.PostAdded);
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostDeletedNotification", b =>
                {
                    b.HasBaseType("WowsKarma.Api.Data.Models.Notifications.NotificationBase");

                    b.Property<Guid>("PostId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("uuid");

                    b.HasIndex("PostId");

                    b.HasDiscriminator().HasValue(NotificationType.PostDeleted);
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostEditedNotification", b =>
                {
                    b.HasBaseType("WowsKarma.Api.Data.Models.Notifications.NotificationBase");

                    b.Property<Guid>("PostId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("uuid");

                    b.HasIndex("PostId");

                    b.HasDiscriminator().HasValue(NotificationType.PostEdited);
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostModDeletedNotification", b =>
                {
                    b.HasBaseType("WowsKarma.Api.Data.Models.Notifications.NotificationBase");

                    b.Property<Guid>("ModActionId")
                        .HasColumnType("uuid");

                    b.HasIndex("ModActionId");

                    b.HasDiscriminator().HasValue(NotificationType.PostModDeleted);
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostModEditedNotification", b =>
                {
                    b.HasBaseType("WowsKarma.Api.Data.Models.Notifications.NotificationBase");

                    b.Property<Guid>("ModActionId")
                        .HasColumnType("uuid");

                    b.HasIndex("ModActionId");

                    b.ToTable("Notifications", t =>
                        {
                            t.Property("ModActionId")
                                .HasColumnName("PostModEditedNotification_ModActionId");
                        });

                    b.HasDiscriminator().HasValue(NotificationType.PostModEdited);
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.ClanMember", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Clan", "Clan")
                        .WithMany("Members")
                        .HasForeignKey("ClanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WowsKarma.Api.Data.Models.Player", "Player")
                        .WithOne("ClanMember")
                        .HasForeignKey("WowsKarma.Api.Data.Models.ClanMember", "PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Clan");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.NotificationBase", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Player", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.PlatformBan", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Player", "Mod")
                        .WithMany()
                        .HasForeignKey("ModId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WowsKarma.Api.Data.Models.Player", "User")
                        .WithMany("PlatformBans")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Mod");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Post", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Player", "Author")
                        .WithMany("PostsSent")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("WowsKarma.Api.Data.Models.Player", "Player")
                        .WithMany("PostsReceived")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("WowsKarma.Api.Data.Models.Replays.Replay", "Replay")
                        .WithOne("Post")
                        .HasForeignKey("WowsKarma.Api.Data.Models.Post", "ReplayId");

                    b.Navigation("Author");

                    b.Navigation("Player");

                    b.Navigation("Replay");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.PostModAction", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Player", "Mod")
                        .WithMany()
                        .HasForeignKey("ModId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WowsKarma.Api.Data.Models.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Mod");

                    b.Navigation("Post");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PlatformBanNotification", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.PlatformBan", "Ban")
                        .WithMany()
                        .HasForeignKey("BanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ban");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostAddedNotification", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostDeletedNotification", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostEditedNotification", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostModDeletedNotification", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.PostModAction", "ModAction")
                        .WithMany()
                        .HasForeignKey("ModActionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ModAction");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Notifications.PostModEditedNotification", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.PostModAction", "ModAction")
                        .WithMany()
                        .HasForeignKey("ModActionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ModAction");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Clan", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Player", b =>
                {
                    b.Navigation("ClanMember");

                    b.Navigation("PlatformBans");

                    b.Navigation("PostsReceived");

                    b.Navigation("PostsSent");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Replays.Replay", b =>
                {
                    b.Navigation("Post")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
