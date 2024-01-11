using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowsKarma.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReplayMinimapRendered : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MinimapRendered",
                table: "Replays",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimapRendered",
                table: "Replays");
        }
    }
}
