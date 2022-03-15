using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class ReplaceClanMemberNavigation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClanMembers",
                table: "ClanMembers");

            migrationBuilder.DropIndex(
                name: "IX_ClanMembers_PlayerId",
                table: "ClanMembers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ClanMembers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClanMembers",
                table: "ClanMembers",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClanMembers",
                table: "ClanMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ClanMembers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClanMembers",
                table: "ClanMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClanMembers_PlayerId",
                table: "ClanMembers",
                column: "PlayerId",
                unique: true);
        }
    }
}
