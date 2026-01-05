using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilens.Migrations
{
    /// <inheritdoc />
    public partial class GroupUserFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "Groups",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_UserID_Name",
                table: "Groups",
                columns: new[] { "UserID", "Name" },
                unique: true,
                filter: "[UserID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_AspNetUsers_UserID",
                table: "Groups",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_AspNetUsers_UserID",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_UserID_Name",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Groups");
        }
    }
}