using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class AddClanMembersTableNavigation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClanMember_Clans_ClanId",
                table: "ClanMember");

            migrationBuilder.DropForeignKey(
                name: "FK_ClanMember_Players_PlayerId",
                table: "ClanMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClanMember",
                table: "ClanMember");

            migrationBuilder.RenameTable(
                name: "ClanMember",
                newName: "ClanMembers");

            migrationBuilder.RenameIndex(
                name: "IX_ClanMember_PlayerId",
                table: "ClanMembers",
                newName: "IX_ClanMembers_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_ClanMember_ClanId",
                table: "ClanMembers",
                newName: "IX_ClanMembers_ClanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClanMembers",
                table: "ClanMembers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClanMembers_Clans_ClanId",
                table: "ClanMembers",
                column: "ClanId",
                principalTable: "Clans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClanMembers_Players_PlayerId",
                table: "ClanMembers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClanMembers_Clans_ClanId",
                table: "ClanMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ClanMembers_Players_PlayerId",
                table: "ClanMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClanMembers",
                table: "ClanMembers");

            migrationBuilder.RenameTable(
                name: "ClanMembers",
                newName: "ClanMember");

            migrationBuilder.RenameIndex(
                name: "IX_ClanMembers_PlayerId",
                table: "ClanMember",
                newName: "IX_ClanMember_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_ClanMembers_ClanId",
                table: "ClanMember",
                newName: "IX_ClanMember_ClanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClanMember",
                table: "ClanMember",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClanMember_Clans_ClanId",
                table: "ClanMember",
                column: "ClanId",
                principalTable: "Clans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClanMember_Players_PlayerId",
                table: "ClanMember",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
