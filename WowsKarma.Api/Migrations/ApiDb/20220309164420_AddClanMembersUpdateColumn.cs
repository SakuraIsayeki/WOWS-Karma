using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace WowsKarma.Api.Migrations.ApiDb
{
    public partial class AddClanMembersUpdateColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "MembersUpdatedAt",
                table: "Clans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MembersUpdatedAt",
                table: "Clans");
        }
    }
}
