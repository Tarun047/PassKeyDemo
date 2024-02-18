using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PassKeys.Business.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditableSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Credentials",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AddColumn<string>(
                name: "LastUsedPlatformInfo",
                table: "Credentials",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Credentials",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "LastUsedPlatformInfo",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Credentials");
        }
    }
}
