using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Pilens.Data;
using Pilens.Data.DTO;
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

        List<GroupDTO> groups = new();
        protected override async Task OnInitializedAsync()
        {
            var test = new Group { Name = "Test" };
            groups = await Db.Groups.Select(g => new GroupDTO(g)).ToListAsync();
        }
        private IEnumerable<GroupDTO> selectedGroups = new HashSet<GroupDTO>();
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
                Identifier = todoTask.Identifier,
                SessionsRequired = todoTask.SessionsRequired,
                ProgressTargetUnits = todoTask.ProgressTargetUnits,
                ProgressCurrentUnits = todoTask.ProgressCurrentUnits,
                ProgressUnitType = todoTask.ProgressUnitType
            };

            Db.ToDoTasks.Add(task);
            await Db.SaveChangesAsync();
            foreach (var groupDto in selectedGroups)
            {
                var toDoTaskGroup = new ToDoTaskGroup
                {
                    ToDoTaskId = task.Id,
                    GroupId = groupDto.Id
                };
                Db.ToDoTaskGroups.Add(toDoTaskGroup);
            }
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
