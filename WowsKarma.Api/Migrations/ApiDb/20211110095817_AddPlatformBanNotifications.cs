using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb;

public partial class AddPlatformBanNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:mod_action_type", "deletion,update")
            .Annotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted,platform_ban")
            .OldAnnotation("Npgsql:Enum:mod_action_type", "deletion,update")
            .OldAnnotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted");

        migrationBuilder.AddColumn<Guid>(
            name: "BanId",
            table: "Notifications",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_BanId",
            table: "Notifications",
            column: "BanId");

        migrationBuilder.AddForeignKey(
            name: "FK_Notifications_PlatformBans_BanId",
            table: "Notifications",
            column: "BanId",
            principalTable: "PlatformBans",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Notifications_PlatformBans_BanId",
            table: "Notifications");

        migrationBuilder.DropIndex(
            name: "IX_Notifications_BanId",
            table: "Notifications");

        migrationBuilder.DropColumn(
            name: "BanId",
            table: "Notifications");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:mod_action_type", "deletion,update")
            .Annotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted")
            .OldAnnotation("Npgsql:Enum:mod_action_type", "deletion,update")
            .OldAnnotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted,platform_ban");
    }
}