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

        List<Group> groups = new();
        protected override async Task OnInitializedAsync()
        {
            groups = await Db.Groups.ToListAsync();
        }
        private IEnumerable<Group> selectedGroups = new HashSet<Group>();
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
                Groups = selectedGroups.ToList(),
                Identifier = todoTask.Identifier,
                SessionsRequired = todoTask.SessionsRequired,
                ProgressTargetUnits = todoTask.ProgressTargetUnits,
                ProgressCurrentUnits = todoTask.ProgressCurrentUnits,
                ProgressUnitType = todoTask.ProgressUnitType
            };
            //var todotaskgroup = new ToDoTaskGroup
            //{
            //    ToDoTaskId = task.Id,
            //    Groups = selectedGroups.id.ToList()
            //};
            Db.ToDoTasks.Add(task);
            //Db.ToDoTasksGroups.Add();
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
