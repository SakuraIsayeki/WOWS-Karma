﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WowsKarma.Api.Data;

namespace WowsKarma.Api.Migrations
{
    [DbContext(typeof(ApiDbContext))]
    [Migration("20201225094937_AddLastUpdatedColumn")]
    partial class AddLastUpdatedColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Player", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastUpdated")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("WgAccountCreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("WgKarma")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<long?>("AuthorId")
                        .HasColumnType("bigint");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NegativeKarma")
                        .HasColumnType("int");

                    b.Property<long?>("PlayerId")
                        .HasColumnType("bigint");

                    b.Property<int>("PositiveKarma")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("PlayerId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Post", b =>
                {
                    b.HasOne("WowsKarma.Api.Data.Models.Player", "Author")
                        .WithMany("PostsSent")
                        .HasForeignKey("AuthorId");

                    b.HasOne("WowsKarma.Api.Data.Models.Player", "Player")
                        .WithMany("PostsReceived")
                        .HasForeignKey("PlayerId");

                    b.Navigation("Author");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("WowsKarma.Api.Data.Models.Player", b =>
                {
                    b.Navigation("PostsReceived");

                    b.Navigation("PostsSent");
                });
#pragma warning restore 612, 618
        }
    }
}
