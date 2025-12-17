using Microsoft.AspNetCore.Components;
using Pilens.Data;
using Pilens.Data.Models;
using System;
using System.Threading.Tasks;

namespace Pilens.Components.Pages.todo
{
    public partial class ToDoUpdate
    {
        [Inject]
        private ApplicationDbContext Db { get; set; } = default;
        [Parameter]
        public int TaskId { get; set; }

        private ToDoTask todoTask = new();

        private string? ErrorMessage { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var task = await Db.ToDoTasks.FindAsync(TaskId);
            if (task == null)
            {
                ErrorMessage = "Task not found.";
                todoTask = new ToDoTask();
                return;
            }

            ErrorMessage = null;
            todoTask = task;
        }

        private async Task UpdateTask()
        {
            try
            {
                Db.ToDoTasks.Update(todoTask);
                await Db.SaveChangesAsync();
                Navigation.NavigateTo("/");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unable to save changes: {ex.Message}";
            }
        }

        private void Cancel()
        {
            Navigation.NavigateTo("/ToDo");
        }
    }
}
