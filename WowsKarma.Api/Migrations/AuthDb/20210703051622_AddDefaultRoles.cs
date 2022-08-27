using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WowsKarma.Api.Migrations.AuthDb;

public partial class AddDefaultRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "InternalName",
            schema: "auth",
            table: "Roles",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DisplayName",
            schema: "auth",
            table: "Roles",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<byte>(
                name: "Id",
                schema: "auth",
                table: "Roles",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint")
            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        migrationBuilder.InsertData(
            schema: "auth",
            table: "Roles",
            columns: new[] { "Id", "DisplayName", "InternalName" },
            values: new object[,]
            {
                { (byte)1, "Administrator", "admin" },
                { (byte)2, "Community Manager", "mod" }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "auth",
            table: "Roles",
            keyColumn: "Id",
            keyValue: (byte)1);

        migrationBuilder.DeleteData(
            schema: "auth",
            table: "Roles",
            keyColumn: "Id",
            keyValue: (byte)2);

        migrationBuilder.AlterColumn<string>(
            name: "InternalName",
            schema: "auth",
            table: "Roles",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "DisplayName",
            schema: "auth",
            table: "Roles",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<byte>(
                name: "Id",
                schema: "auth",
                table: "Roles",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint")
            .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
    }
}