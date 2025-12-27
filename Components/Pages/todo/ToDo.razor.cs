using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pilens.Components.Pages.todo
{
    public partial class ToDo
    {
        private const string SessionsMustBePositiveMessage = "Sesiju skaitam jābūt pozitīvam.";

        [Inject]
        private ApplicationDbContext Db { get; set; } = default;

        [Parameter]
        public EventCallback<int> OnStartPomodoro { get; set; }

        private List<ToDoTask> Items { get; set; } = new();
        private string? ErrorMessage { get; set; }
        private string NewGroupName { get; set; } = string.Empty;

        private int TotalSessions => Items
            .Where(task => task.Identifier == "Sesijas")
            .Sum(task => task.SessionsRequired);

        private bool CanStartPomodoro => TotalSessions > 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadTasksAsync();
        }

        private async Task LoadTasksAsync()
        {
            try
            {
                Items = await Db.ToDoTasks
                    .AsNoTracking()
                    .OrderBy(t => t.Identifier)
                    .ThenBy(t => t.Title)
                    .ToListAsync();
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Tasks could not be loaded: {ex.Message}";
            }
        }

        private double GetProgressPercent(ToDoTask task)
        {
            if (task.ProgressTargetUnits <= 0)
            {
                return 0;
            }

            return Math.Clamp((double)task.ProgressCurrentUnits / task.ProgressTargetUnits * 100.0, 0, 100);
        }

        private async Task ItemUpdated(MudItemDropInfo<ToDoTask> dropItem)
        {
            if (dropItem?.Item == null || string.IsNullOrWhiteSpace(dropItem.DropzoneIdentifier))
            {
                return;
            }

            var entity = await Db.ToDoTasks.FindAsync(dropItem.Item.Id);
            if (entity == null)
            {
                return;
            }

            entity.Identifier = dropItem.DropzoneIdentifier;

            var minutes = (int)Math.Ceiling(entity.EffortDuration.TotalMinutes);
            if (dropItem.DropzoneIdentifier == "Sesijas")
            {
                const int pomodoroMinutes = 25;
                entity.SessionsRequired = minutes > 0
                    ? (int)Math.Ceiling(minutes / (double)pomodoroMinutes)
                    : 0;
            }
            else
            {
                entity.SessionsRequired = 0;
            }

            await Db.SaveChangesAsync();
            await LoadTasksAsync();
        }

        private async Task StartPomodoroFromDropzoneAsync()
        {
            if (!CanStartPomodoro)
            {
                ErrorMessage = SessionsMustBePositiveMessage;
                return;
            }

            ErrorMessage = null;

            if (OnStartPomodoro.HasDelegate)
            {
                await OnStartPomodoro.InvokeAsync(TotalSessions);
            }
        }

        private async Task ToggleCompletion(ToDoTask task)
        {
            var entity = await Db.ToDoTasks.FindAsync(task.Id);
            if (entity == null)
            {
                return;
            }

            entity.IsCompleted = !(entity.IsCompleted);
            await Db.SaveChangesAsync();
            StateHasChanged();
        }

        private void StartEdit(ToDoTask task)
        {
            Navigation.NavigateTo($"/ToDo/update/{task.Id}");
        }

        private async Task RemoveTask(ToDoTask task)
        {
            var entity = await Db.ToDoTasks.FindAsync(task.Id);
            if (entity == null)
            {
                return;
            }


            Db.ToDoTasks.Remove(entity);
            await Db.SaveChangesAsync();
            await LoadTasksAsync();
        }

        private async Task CreateGroupAsync()
        {
            var trimmedName = NewGroupName?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                ErrorMessage = "Grupas nosaukums ir obligāts.";
                return;
            }

            try
            {
                var exists = await Db.Groups.AnyAsync(group => group.Name == trimmedName);
                if (exists)
                {
                    ErrorMessage = "Grupa ar šādu nosaukumu jau pastāv.";
                    return;
                }

                Db.Groups.Add(new Group { Name = trimmedName });
                await Db.SaveChangesAsync();

                NewGroupName = string.Empty;
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Grupu nevarēja izveidot: {ex.Message}";
            }
        }

        private void NavigateToCreate()
        {
            Navigation.NavigateTo($"ToDo/create");
        }
    }
}

