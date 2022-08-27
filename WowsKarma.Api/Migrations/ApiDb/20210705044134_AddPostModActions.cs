using Microsoft.EntityFrameworkCore.Migrations;

namespace WowsKarma.Api.Migrations.ApiDb;

public partial class AddPostModActions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:mod_action_type", "deletion,update");

        migrationBuilder.CreateTable(
            name: "PostModActions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PostId = table.Column<Guid>(type: "uuid", nullable: false),
                ActionType = table.Column<ModActionType>(type: "mod_action_type", nullable: false),
                ModId = table.Column<long>(type: "bigint", nullable: false),
                Reason = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PostModActions", x => x.Id);
                table.ForeignKey(
                    name: "FK_PostModActions_Players_ModId",
                    column: x => x.ModId,
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PostModActions_Posts_PostId",
                    column: x => x.PostId,
                    principalTable: "Posts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PostModActions_ModId",
            table: "PostModActions",
            column: "ModId");

        migrationBuilder.CreateIndex(
            name: "IX_PostModActions_PostId",
            table: "PostModActions",
            column: "PostId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PostModActions");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:Enum:mod_action_type", "deletion,update");
    }
}