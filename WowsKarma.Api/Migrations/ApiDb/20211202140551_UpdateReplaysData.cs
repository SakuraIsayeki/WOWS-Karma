using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using WowsKarma.Api.Data.Models.Replays;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class UpdateReplaysData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<IEnumerable<ReplayChatMessage>>(
                name: "ChatMessages",
                table: "Replays",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<IEnumerable<ReplayPlayer>>(
                name: "Players",
                table: "Replays",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatMessages",
                table: "Replays");

            migrationBuilder.DropColumn(
                name: "Players",
                table: "Replays");
        }
    }
}
