using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project3.Migrations
{
    /// <inheritdoc />
    public partial class RenameFileMetaDataColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Expiration",
                table: "FileMetaDatas",
                newName: "ExpirationDays");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "FileMetaDatas",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpirationDays",
                table: "FileMetaDatas",
                newName: "Expiration");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "FileMetaDatas",
                newName: "CreatedDate");
        }
    }
}
