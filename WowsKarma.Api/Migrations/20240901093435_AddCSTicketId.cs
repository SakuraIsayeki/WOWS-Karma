using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowsKarma.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCSTicketId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerSupportTicketId",
                table: "Posts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerSupportTicketId",
                table: "Posts");
        }
    }
}
