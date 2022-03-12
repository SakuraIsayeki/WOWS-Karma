using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class AddClans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:clan_role", "unknown,commander,executive_officer,recruitment_officer,commissioned_officer,officer,private")
                .Annotation("Npgsql:Enum:mod_action_type", "deletion,update")
                .Annotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted,platform_ban")
                .OldAnnotation("Npgsql:Enum:mod_action_type", "deletion,update")
                .OldAnnotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted,platform_ban");

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LeagueColor = table.Column<long>(type: "bigint", nullable: false),
                    IsDisbanded = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MembersUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClanMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    ClanId = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    LeftAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Role = table.Column<ClanRole>(type: "clan_role", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClanMembers_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClanMembers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClanMembers_ClanId",
                table: "ClanMembers",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_ClanMembers_PlayerId",
                table: "ClanMembers",
                column: "PlayerId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanMembers");

            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:mod_action_type", "deletion,update")
                .Annotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted,platform_ban")
                .OldAnnotation("Npgsql:Enum:clan_role", "unknown,commander,executive_officer,recruitment_officer,commissioned_officer,officer,private")
                .OldAnnotation("Npgsql:Enum:mod_action_type", "deletion,update")
                .OldAnnotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted,platform_ban");
        }
    }
}
