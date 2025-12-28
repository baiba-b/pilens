using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.DTO;
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
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

        [Parameter]
        public EventCallback<int> OnStartPomodoro { get; set; }

        private List<ToDoTask> Items { get; set; } = new();
        private string? ErrorMessage { get; set; }
        private string NewGroupName { get; set; } = string.Empty;
        private string userId { get; set; }

        // TODO: kļūdas apstrāde
        private int TotalSessions => Items
            .Where(task => task.Identifier == "Sesijas")
            .Sum(task => task.SessionsRequired);

        private bool CanStartPomodoro => TotalSessions > 0;

        
        protected override async Task OnInitializedAsync()
        {
            userId = await getUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }
          
            await LoadTasksAsync();
        }
        private async Task LoadTasksAsync()
        {
            try
            {
               
                await using var db = await DbContextFactory.CreateDbContextAsync();
                Items = await db.ToDoTasks
                    .AsNoTracking()
                    .Where(t => t.UserID == userId)
                    .OrderBy(t => t.Identifier)
                    .ThenBy(t => t.Title)
                    .ToListAsync();
                ErrorMessage = null;
            }
            catch (Exception)
            {
                string errorMessage = "Neizdevās atrast tavus uzdevumus!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
        }
        private double GetProgressPercent(ToDoTask task)
        {
            if (task == null)
            {
                string errorMessage = "Neizdevās atrast uzdevumu!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return 0;
            }
            if (task.ProgressTargetUnits <= 0)
            {
                return 0;
            }

            return Math.Clamp((double)task.ProgressCurrentUnits / task.ProgressTargetUnits * 100.0, 0, 100);
        }
        private async Task ItemUpdated(MudItemDropInfo<ToDoTask> dropItem)
        {
            try
            {
                if (dropItem?.Item == null || string.IsNullOrWhiteSpace(dropItem.DropzoneIdentifier))
                {
                    return;
                }

                await using var db = await DbContextFactory.CreateDbContextAsync();
                var entity = await db.ToDoTasks.FindAsync(dropItem.Item.Id);
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

                await db.SaveChangesAsync();
                await LoadTasksAsync();
            }
            catch (Exception)
            {
                string errorMessage = "Neizdevās atjaunot uzdevumu!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
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
            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var entity = await db.ToDoTasks.FindAsync(task.Id);
                if (entity == null)
                {
                    string errorMessage = "Neizdevās atrast uzdevumu!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                }

                entity.IsCompleted = !entity.IsCompleted;
                await db.SaveChangesAsync();
                StateHasChanged();
            }
            catch (Exception)
            {
                string errorMessage = "Neizdevās pārslēgt uzdevuma statusu!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
        }
          
        private void StartEdit(ToDoTask task)
        {
            Navigation.NavigateTo($"/ToDo/update/{task.Id}");
        }
        private async Task RemoveTask(ToDoTask task)
        {
            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var entity = await db.ToDoTasks.FindAsync(task.Id);
                if (entity == null)
                {
                    string errorMessage = "Neizdevās atrast uzdevumu!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                }

                db.ToDoTasks.Remove(entity);
                await db.SaveChangesAsync();
                await LoadTasksAsync();
            }
            catch (Exception)
            {
                string errorMessage = "Neizdevās izdzēst uzdevumu!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
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
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var exists = await db.Groups.AnyAsync(group => group.Name == trimmedName);
                if (exists)
                {
                    ErrorMessage = "Grupa ar šādu nosaukumu jau pastāv.";
                    return;
                }

                db.Groups.Add(new Group { Name = trimmedName });
                await db.SaveChangesAsync();

                NewGroupName = string.Empty;
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Grupu nevarēja izveidot: {ex.Message}";
            }
        }
        // TODO: kļūdas apstrāde
        async Task<string> getUserId()
        {
            try
            {
                var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
                var UserId = user.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;
                return UserId;
            }
            catch (Exception)
            {
                string errorMessage = "Neizdevās identificēt lietotāju.";
                SnackbarService.Add(errorMessage, Severity.Error);
                return string.Empty;
            }
        }
        private void NavigateToCreate()
        {
            Navigation.NavigateTo($"ToDo/create");
        }
    }
}

