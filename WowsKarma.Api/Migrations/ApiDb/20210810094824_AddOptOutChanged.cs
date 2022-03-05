using Microsoft.EntityFrameworkCore.Migrations;

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class AddOptOutChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OptOutChanged",
                table: "Players",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptOutChanged",
                table: "Players");
        }
    }
}
