using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilens.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pomodoros_UserID",
                table: "Pomodoros");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "ToDoTasks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Pomodoros_UserID",
                table: "Pomodoros",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoTasks_AspNetUsers_UserID",
                table: "ToDoTasks",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoTasks_AspNetUsers_UserID",
                table: "ToDoTasks");

            migrationBuilder.DropIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks");

            migrationBuilder.DropIndex(
                name: "IX_Pomodoros_UserID",
                table: "Pomodoros");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "ToDoTasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Pomodoros_UserID",
                table: "Pomodoros",
                column: "UserID",
                unique: true);
        }
    }
}
