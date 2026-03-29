using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EleveRats.Migrations
{
    /// <inheritdoc />
    public partial class Birthdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "profiles");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "profiles",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "profiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "profiles",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "profiles",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "profiles",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "profiles");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
