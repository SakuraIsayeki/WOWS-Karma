using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WowsKarma.Api.Migrations
{
    public partial class EditPostModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NegativeKarma",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "PositiveKarma",
                table: "Posts",
                newName: "PostFlairs");

            migrationBuilder.RenameColumn(
                name: "WgKarma",
                table: "Players",
                newName: "SiteKarma");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "Players",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<long>(
                name: "PlayerId",
                table: "Posts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AuthorId",
                table: "Posts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Posts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<bool>(
                name: "NegativeKarmaAble",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Posts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "GameKarma",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Players",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "NegativeKarmaAble",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "GameKarma",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "PostFlairs",
                table: "Posts",
                newName: "PositiveKarma");

            migrationBuilder.RenameColumn(
                name: "SiteKarma",
                table: "Players",
                newName: "WgKarma");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Players",
                newName: "LastUpdated");

            migrationBuilder.AlterColumn<long>(
                name: "PlayerId",
                table: "Posts",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "AuthorId",
                table: "Posts",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "NegativeKarma",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
