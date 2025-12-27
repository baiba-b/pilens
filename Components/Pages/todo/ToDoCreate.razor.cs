using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.DTO;
using Pilens.Data.Models;

namespace Pilens.Components.Pages.todo
{
    public partial class ToDoCreate
    {
        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

        private ToDoTaskDTO todoTaskDto = new();

        private string userId {  get; set; }
        private string? ErrorMessage { get; set; }

        protected override void OnInitialized()
        {
            todoTaskDto = new ToDoTaskDTO();
        }

        List<GroupDTO> groups = new();

        protected override async Task OnInitializedAsync()
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();
            groups = await db.Groups
                .Select(g => new GroupDTO(g))
                .ToListAsync();
            var userId = await getUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                SnackbarService.Add("Neizdevās identificēt lietotāju.", Severity.Error);
                return;
            }
            todoTaskDto.UserID = userId;

        }

        private IEnumerable<GroupDTO> selectedGroups = new HashSet<GroupDTO>();

        private async Task CreateTask()
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();

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
                ProgressUnitType = todoTaskDto.ProgressUnitType,
                UserID = todoTaskDto.UserID
            };

            db.ToDoTasks.Add(task);
            await db.SaveChangesAsync();

            foreach (var groupDto in selectedGroups)
            {
                var toDoTaskGroup = new ToDoTaskGroup
                {
                    ToDoTaskId = task.Id,
                    GroupId = groupDto.Id
                };
                db.ToDoTaskGroups.Add(toDoTaskGroup);
            }

            await db.SaveChangesAsync();
            Navigation.NavigateTo("/");
            todoTaskDto = new ToDoTaskDTO();
        }

        private void Cancel()
        {
            Navigation.NavigateTo("/ToDo");
        }
        async Task<string> getUserId()
        {
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
            var UserId = user.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;
            return UserId;
        }

    }
}
