using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataShare_API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToFileMetaDataRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "FileMetaDatas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FileMetaDatas_UserId",
                table: "FileMetaDatas",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileMetaDatas_Users_UserId",
                table: "FileMetaDatas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileMetaDatas_Users_UserId",
                table: "FileMetaDatas");

            migrationBuilder.DropIndex(
                name: "IX_FileMetaDatas_UserId",
                table: "FileMetaDatas");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FileMetaDatas");
        }
    }
}
