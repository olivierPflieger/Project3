using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataShare_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExpirationColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "FileMetaDatas");

            migrationBuilder.AddColumn<int>(
                name: "Expiration",
                table: "FileMetaDatas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "FileMetaDatas");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "FileMetaDatas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
