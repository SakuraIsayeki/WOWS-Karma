using Microsoft.EntityFrameworkCore.Migrations;

namespace WowsKarma.Api.Migrations
{
    public partial class AddPlayerHidden : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WgHidden",
                table: "Players",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WgHidden",
                table: "Players");
        }
    }
}
