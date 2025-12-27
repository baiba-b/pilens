using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilens.Migrations
{
    /// <inheritdoc />
    public partial class ToDoTaskAddUserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInMinutes",
                table: "Pomodoros");

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "ToDoTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "ToDoTasks");

            migrationBuilder.AddColumn<int>(
                name: "DurationInMinutes",
                table: "Pomodoros",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
