using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb;

public partial class AddModEditedNotification : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "PostModEditedNotification_ModActionId",
            table: "Notifications",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_PostModEditedNotification_ModActionId",
            table: "Notifications",
            column: "PostModEditedNotification_ModActionId");

        migrationBuilder.AddForeignKey(
            name: "FK_Notifications_PostModActions_PostModEditedNotification_ModA~",
            table: "Notifications",
            column: "PostModEditedNotification_ModActionId",
            principalTable: "PostModActions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Notifications_PostModActions_PostModEditedNotification_ModA~",
            table: "Notifications");

        migrationBuilder.DropIndex(
            name: "IX_Notifications_PostModEditedNotification_ModActionId",
            table: "Notifications");

        migrationBuilder.DropColumn(
            name: "PostModEditedNotification_ModActionId",
            table: "Notifications");
    }
}