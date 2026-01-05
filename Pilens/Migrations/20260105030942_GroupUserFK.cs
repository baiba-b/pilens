using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilens.Migrations
{
    /// <inheritdoc />
    public partial class GroupUserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "Groups",
                type: "nvarchar(450)",
                nullable: true);

            //AI kods, mēģinot salabot update-database nestrādāšanu
            migrationBuilder.Sql(@"
DECLARE @uid nvarchar(450) = (SELECT TOP 1 Id FROM AspNetUsers ORDER BY Id);
IF @uid IS NULL
    THROW 51000, 'No users found to backfill Groups.UserID', 1;
UPDATE Groups SET UserID = @uid WHERE UserID IS NULL OR UserID = '';
");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "Groups",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_UserID",
                table: "Groups",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_AspNetUsers_UserID",
                table: "Groups",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_AspNetUsers_UserID",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_UserID",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Groups");
        }
    }
}
