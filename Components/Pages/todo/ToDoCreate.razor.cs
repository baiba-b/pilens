using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.Models;
namespace Pilens.Components.Pages.todo
{
    
    public partial class ToDoCreate
    {
        [Inject]
        private ApplicationDbContext Db { get; set; } = default;


        private ToDoTask todoTask = new();

        private string? ErrorMessage { get; set; }
        protected override void OnInitialized()
        {
            todoTask = new ToDoTask();
        }

        private async Task CreateTask()
        {
            var task = new ToDoTask
            {
                Title = todoTask.Title,
                Description = todoTask.Description,
                IsCompleted = todoTask.IsCompleted,
                Effort = todoTask.Effort,
                Deadline = todoTask.Deadline,
                EffortDuration = todoTask.EffortDuration,
                Groups = todoTask.Groups.ToList(),
                Identifier = todoTask.Identifier,
                SessionsRequired = todoTask.SessionsRequired,
                ProgressTargetUnits = todoTask.ProgressTargetUnits,
                ProgressCurrentUnits = todoTask.ProgressCurrentUnits,
                ProgressUnitType = todoTask.ProgressUnitType
            };

            Db.ToDoTasks.Add(task);
            await Db.SaveChangesAsync();
            Navigation.NavigateTo($"/");
            todoTask = new ToDoTask();
        }

        private void Cancel()
        {
            Navigation.NavigateTo("/ToDo");
        }
    }
}
