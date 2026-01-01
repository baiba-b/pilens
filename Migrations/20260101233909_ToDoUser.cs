using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilens.Migrations
{
    /// <inheritdoc />
    public partial class ToDoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks");

            migrationBuilder.DropIndex(
                name: "IX_Pomodoros_UserID",
                table: "Pomodoros");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pomodoros_UserID",
                table: "Pomodoros",
                column: "UserID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks");

            migrationBuilder.DropIndex(
                name: "IX_Pomodoros_UserID",
                table: "Pomodoros");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Pomodoros_UserID",
                table: "Pomodoros",
                column: "UserID");
        }
    }
}
