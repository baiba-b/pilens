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
        [Inject]
        private ApplicationDbContext Db { get; set; } = default;

        private List<ToDoTask> Items { get; set; } = new();
        private string? ErrorMessage { get; set; }

        private int TotalSessions => Items
            .Where(task => task.Identifier == "Sesijas")
            .Sum(task => task.SessionsRequired);

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

        private void NavigateToCreate()
        {
             Navigation.NavigateTo($"ToDo/create");
        }
    }
}

