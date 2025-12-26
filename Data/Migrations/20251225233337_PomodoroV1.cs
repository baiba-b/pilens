using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilens.Migrations
{
    /// <inheritdoc />
    public partial class PomodoroV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdjustedMin",
                table: "Pomodoros",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LongPauseMinutes",
                table: "Pomodoros",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minutes",
                table: "Pomodoros",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PauseMinutes",
                table: "Pomodoros",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionAmount",
                table: "Pomodoros",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionLongPause",
                table: "Pomodoros",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustedMin",
                table: "Pomodoros");

            migrationBuilder.DropColumn(
                name: "LongPauseMinutes",
                table: "Pomodoros");

            migrationBuilder.DropColumn(
                name: "Minutes",
                table: "Pomodoros");

            migrationBuilder.DropColumn(
                name: "PauseMinutes",
                table: "Pomodoros");

            migrationBuilder.DropColumn(
                name: "SessionAmount",
                table: "Pomodoros");

            migrationBuilder.DropColumn(
                name: "SessionLongPause",
                table: "Pomodoros");
        }
    }
}
