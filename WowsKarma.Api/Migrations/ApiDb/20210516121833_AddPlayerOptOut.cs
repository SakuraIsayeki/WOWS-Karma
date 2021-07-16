using Microsoft.EntityFrameworkCore.Migrations;

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class AddPlayerOptOut : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OptedOut",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptedOut",
                table: "Players");
        }
    }
}
