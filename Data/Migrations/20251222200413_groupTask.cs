using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilens.Migrations
{
    /// <inheritdoc />
    public partial class groupTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoTaskGroups_Groups_GroupsId",
                table: "ToDoTaskGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ToDoTaskGroups_ToDoTasks_ToDoTasksId",
                table: "ToDoTaskGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ToDoTaskGroups",
                table: "ToDoTaskGroups");

            migrationBuilder.RenameColumn(
                name: "ToDoTasksId",
                table: "ToDoTaskGroups",
                newName: "ToDoTaskId");

            migrationBuilder.RenameColumn(
                name: "GroupsId",
                table: "ToDoTaskGroups",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ToDoTaskGroups_ToDoTasksId",
                table: "ToDoTaskGroups",
                newName: "IX_ToDoTaskGroups_ToDoTaskId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ToDoTaskGroups",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ToDoTaskGroups",
                table: "ToDoTaskGroups",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoTaskGroups_GroupId",
                table: "ToDoTaskGroups",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoTaskGroups_Groups_GroupId",
                table: "ToDoTaskGroups",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoTaskGroups_ToDoTasks_ToDoTaskId",
                table: "ToDoTaskGroups",
                column: "ToDoTaskId",
                principalTable: "ToDoTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoTaskGroups_Groups_GroupId",
                table: "ToDoTaskGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ToDoTaskGroups_ToDoTasks_ToDoTaskId",
                table: "ToDoTaskGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ToDoTaskGroups",
                table: "ToDoTaskGroups");

            migrationBuilder.DropIndex(
                name: "IX_ToDoTaskGroups_GroupId",
                table: "ToDoTaskGroups");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ToDoTaskGroups");

            migrationBuilder.RenameColumn(
                name: "ToDoTaskId",
                table: "ToDoTaskGroups",
                newName: "ToDoTasksId");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "ToDoTaskGroups",
                newName: "GroupsId");

            migrationBuilder.RenameIndex(
                name: "IX_ToDoTaskGroups_ToDoTaskId",
                table: "ToDoTaskGroups",
                newName: "IX_ToDoTaskGroups_ToDoTasksId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ToDoTaskGroups",
                table: "ToDoTaskGroups",
                columns: new[] { "GroupsId", "ToDoTasksId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoTaskGroups_Groups_GroupsId",
                table: "ToDoTaskGroups",
                column: "GroupsId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoTaskGroups_ToDoTasks_ToDoTasksId",
                table: "ToDoTaskGroups",
                column: "ToDoTasksId",
                principalTable: "ToDoTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
