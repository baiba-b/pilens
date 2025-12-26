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

        private ToDoTaskDTO todoTaskDto = new();

        private string? ErrorMessage { get; set; }

        protected override void OnInitialized()
        {
            todoTaskDto = new ToDoTaskDTO();
        }

        List<GroupDTO> groups = new();

        protected override async Task OnInitializedAsync()
        {
            groups = await Db.Groups
                .Select(g => new GroupDTO(g))
                .ToListAsync();
        }

        private IEnumerable<GroupDTO> selectedGroups = new HashSet<GroupDTO>();

        private async Task CreateTask()
        {
            var task = new ToDoTask
            {
                Title = todoTaskDto.Title,
                Description = todoTaskDto.Description,
                IsCompleted = todoTaskDto.IsCompleted,
                Effort = todoTaskDto.Effort,
                Deadline = todoTaskDto.Deadline,
                EffortDuration = todoTaskDto.EffortDuration,
                Identifier = todoTaskDto.Identifier,
                SessionsRequired = todoTaskDto.SessionsRequired,
                ProgressTargetUnits = todoTaskDto.ProgressTargetUnits,
                ProgressCurrentUnits = todoTaskDto.ProgressCurrentUnits,
                ProgressUnitType = todoTaskDto.ProgressUnitType
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
            Navigation.NavigateTo("/");
            todoTaskDto = new ToDoTaskDTO();
        }

        private void Cancel()
        {
            Navigation.NavigateTo("/ToDo");
        }
    }
}
