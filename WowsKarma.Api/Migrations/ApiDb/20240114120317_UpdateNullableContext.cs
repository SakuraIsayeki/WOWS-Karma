using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Nodsoft.WowsReplaysUnpack.Core.Models;
using WowsKarma.Api.Data.Models.Replays;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
    /// <inheritdoc />
    public partial class UpdateNullableContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<IEnumerable<ReplayPlayer>>(
                name: "Players",
                table: "Replays",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(IEnumerable<ReplayPlayer>),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<IEnumerable<ReplayChatMessage>>(
                name: "ChatMessages",
                table: "Replays",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(IEnumerable<ReplayChatMessage>),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BlobName",
                table: "Replays",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<ArenaInfo>(
                name: "ArenaInfo",
                table: "Replays",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(ArenaInfo),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "PostModActions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Players",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "OptOutChanged",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Tag",
                table: "Clans",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clans",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Clans",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<IEnumerable<ReplayPlayer>>(
                name: "Players",
                table: "Replays",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(IEnumerable<ReplayPlayer>),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<IEnumerable<ReplayChatMessage>>(
                name: "ChatMessages",
                table: "Replays",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(IEnumerable<ReplayChatMessage>),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "BlobName",
                table: "Replays",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<ArenaInfo>(
                name: "ArenaInfo",
                table: "Replays",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(ArenaInfo),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "PostModActions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Players",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "OptOutChanged",
                table: "Players",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Tag",
                table: "Clans",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clans",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Clans",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
