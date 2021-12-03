using System;
using Microsoft.EntityFrameworkCore.Migrations;
using WowsKarma.Api.Data.Models.Replays;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class AddReplays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReplayId",
                table: "Posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Replays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlobName = table.Column<string>(type: "text", nullable: true),
                    ArenaInfo = table.Column<ReplayArenaInfo>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replays", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ReplayId",
                table: "Posts",
                column: "ReplayId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Replays_ReplayId",
                table: "Posts",
                column: "ReplayId",
                principalTable: "Replays",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Replays_ReplayId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "Replays");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ReplayId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ReplayId",
                table: "Posts");
        }
    }
}
