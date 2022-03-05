using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class AddPostModDeletedNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:mod_action_type", "deletion,update")
                .Annotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted")
                .OldAnnotation("Npgsql:Enum:mod_action_type", "deletion,update");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<NotificationType>(type: "notification_type", nullable: false),
                    EmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModActionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Players_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_PostModActions_ModActionId",
                        column: x => x.ModActionId,
                        principalTable: "PostModActions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AccountId",
                table: "Notifications",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ModActionId",
                table: "Notifications",
                column: "ModActionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:mod_action_type", "deletion,update")
                .OldAnnotation("Npgsql:Enum:mod_action_type", "deletion,update")
                .OldAnnotation("Npgsql:Enum:notification_type", "unknown,other,post_added,post_edited,post_deleted,post_mod_edited,post_mod_deleted");
        }
    }
}
