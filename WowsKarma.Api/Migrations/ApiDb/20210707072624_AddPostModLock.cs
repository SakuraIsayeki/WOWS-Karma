using Microsoft.EntityFrameworkCore.Migrations;

namespace WowsKarma.Api.Migrations.ApiDb;

public partial class AddPostModLock : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Posts",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Content",
            table: "Posts",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "ModLocked",
            table: "Posts",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ModLocked",
            table: "Posts");

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Posts",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Content",
            table: "Posts",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");
    }
}