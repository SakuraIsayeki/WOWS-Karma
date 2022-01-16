using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
	public partial class AddPostReactions : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "PostReactions",
				columns: table => new
				{
					PostId = table.Column<Guid>(type: "uuid", nullable: false),
					PlayerId = table.Column<long>(type: "bigint", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PostReactions", x => new { x.PostId, x.PlayerId });
					table.ForeignKey(
						name: "FK_PostReactions_Players_PlayerId",
						column: x => x.PlayerId,
						principalTable: "Players",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PostReactions_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_PostReactions_PlayerId",
				table: "PostReactions",
				column: "PlayerId");

			migrationBuilder.CreateIndex(
				name: "IX_PostReactions_PostId",
				table: "PostReactions",
				column: "PostId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PostReactions");
		}
	}
}
