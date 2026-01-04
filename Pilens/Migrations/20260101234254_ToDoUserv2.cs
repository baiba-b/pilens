using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilens.Migrations
{
    /// <inheritdoc />
    public partial class ToDoUserv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoTasks_UserID",
                table: "ToDoTasks",
                column: "UserID",
                unique: true);
        }
    }
}
