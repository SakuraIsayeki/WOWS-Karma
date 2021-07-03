using Microsoft.EntityFrameworkCore.Migrations;

namespace WowsKarma.Api.Migrations.AuthDb
{
    public partial class FixUserRoleJoins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Users_UserId",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_UserId",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "auth",
                table: "Roles");

            migrationBuilder.CreateTable(
                name: "RoleUser",
                schema: "auth",
                columns: table => new
                {
                    RolesId = table.Column<byte>(type: "smallint", nullable: false),
                    UsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_RolesId",
                        column: x => x.RolesId,
                        principalSchema: "auth",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                schema: "auth",
                table: "RoleUser",
                column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleUser",
                schema: "auth");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                schema: "auth",
                table: "Roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UserId",
                schema: "auth",
                table: "Roles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Users_UserId",
                schema: "auth",
                table: "Roles",
                column: "UserId",
                principalSchema: "auth",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
