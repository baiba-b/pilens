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
using Pilens.Components.Shared;
using Pilens.Data.State;

namespace Pilens.Components.Pages.todo
{
    public partial class ToDo : IDisposable
    {
        private const string SessionsMustBePositiveMessage = "Sesiju skaitam jābūt pozitīvam";

        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private PomodoroState PomodoroState { get; set; } = default!;

        [Parameter]
        public EventCallback<int> OnStartPomodoro { get; set; }

        private List<ToDoTask> Items { get; set; } = new();
        private string? ErrorMessage { get; set; }
        private string NewGroupName { get; set; } = string.Empty;
        private string userId { get; set; }

        private int pomodoroMinutes = 25;

        private int TotalSessions => Items
            .Where(task => task.Identifier == "Sesijas")
            .Sum(task => task.SessionsRequired);

        private bool CanStartPomodoro => TotalSessions > 0;

        
        protected override async Task OnInitializedAsync()
        {
            pomodoroMinutes = PomodoroState.Minutes;
            PomodoroState.OnChange += OnPomodoroMinutesChanged;

            userId = await getUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            await LoadTasksAsync();
            await RecalculateSessionsAsync();
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

        private void OnPomodoroMinutesChanged()
        {
            pomodoroMinutes = PomodoroState.Minutes;
            _ = InvokeAsync(RecalculateSessionsAsync);
            InvokeAsync(StateHasChanged);
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
                    var pomodoroLength = pomodoroMinutes > 0 ? pomodoroMinutes : 25;
                    entity.SessionsRequired = minutes > 0
                        ? (int)Math.Ceiling(minutes / (double)pomodoroLength)
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

        private bool CanIncreaseProgress(ToDoTask task)
        {
            return task?.ProgressTargetUnits > 0 && task.ProgressCurrentUnits < task.ProgressTargetUnits;
        }

        private bool CanDecreaseProgress(ToDoTask task)
        {
            return task?.ProgressTargetUnits > 0 && task.ProgressCurrentUnits > 0;
        }

        private async Task ChangeProgressAsync(ToDoTask task, int changeAmount)
        {
            
            var newValue = task.ProgressCurrentUnits + changeAmount;

            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var entity = await db.ToDoTasks.FindAsync(task.Id);
                if (entity == null)
                {
                    string errorMessage = "Neizdevās atrast uzdevumu!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }

                entity.ProgressCurrentUnits = newValue;
                await db.SaveChangesAsync();

                task.ProgressCurrentUnits = newValue;
                StateHasChanged();
            }
            catch (Exception)
            {
                string errorMessage = "Neizdevās atjaunināt progresu!";
                SnackbarService.Add(errorMessage, Severity.Error);
            }
        }
        /// <summary>
        /// UMF_003 – Izdzēst uzdevumu
        /// Ļauj reģistrētam lietotājam dzēst uzdevumu ar apstiprinājuma dialogu.
        /// </summary>
        /// <param name="task">Dzēšamais uzdevums</param>
        private async Task RemoveTask(ToDoTask task)
        {
            var parameters = new DialogParameters<ConfirmDeleteDialog>
            {
                { x => x.ContentText, $"Vai esi pārliecināts, ka vēlies dzēst \"{task.Title}\"?" },
                { x => x.ButtonText, "Dzēst" },
                { x => x.Color, Color.Error }
            };

            var options = new DialogOptions
            {
                CloseButton = true,
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.ExtraSmall
            };


            var dialogRef = await DialogService.ShowAsync<ConfirmDeleteDialog>("Dzēst uzdevumu", parameters, options);
            var dialogResult = await dialogRef.Result;

            if (dialogResult.Canceled)
            {
                return;
            }

            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var entity = await db.ToDoTasks.FindAsync(task.Id);
                if (entity == null)
                {
                    string errorMessage = "Neizdevās atrast uzdevumu!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }

                var connections = await db.ToDoTaskGroups
                    .Where(connection => connection.ToDoTaskId == entity.Id)
                    .ToListAsync();
                if (connections.Count > 0)
                {
                    db.ToDoTaskGroups.RemoveRange(connections);
                }

                db.ToDoTasks.Remove(entity);
                await db.SaveChangesAsync();
                await LoadTasksAsync();
                SnackbarService.Add("Uzdevums veiksmīgi izdzēsts!", Severity.Success);
            }
            catch (Exception)
            {
                string errorMessage = "Uzdevumu neizdevās izdzēst!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
        }
        private async Task CreateGroupAsync()
        {
            var trimmedName = NewGroupName?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                ErrorMessage = "Grupas nosaukums ir obligāts!";
                return;
            }

            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var exists = await db.Groups.AnyAsync(group => group.Name == trimmedName);
                if (exists)
                {
                    ErrorMessage = "Grupa ar šādu nosaukumu jau pastāv!";
                    return;
                }

                db.Groups.Add(new Group { Name = trimmedName });
                await db.SaveChangesAsync();

                NewGroupName = string.Empty;
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Grupu nevarēja izveidot!";
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

        public void Dispose()
        {
            PomodoroState.OnChange -= OnPomodoroMinutesChanged;
        }

        private async Task RecalculateSessionsAsync()
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var pomodoroLength = pomodoroMinutes > 0 ? pomodoroMinutes : 25;

            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var sessionTasks = await db.ToDoTasks
                    .Where(t => t.UserID == userId && t.Identifier == "Sesijas")
                    .ToListAsync();

                var updated = false;
                foreach (var task in sessionTasks)
                {
                    var minutes = (int)Math.Ceiling(task.EffortDuration.TotalMinutes);
                    var newSessions = minutes > 0
                        ? (int)Math.Ceiling(minutes / (double)pomodoroLength)
                        : 0;

                    if (task.SessionsRequired != newSessions)
                    {
                        task.SessionsRequired = newSessions;
                        updated = true;
                    }
                }

                if (updated)
                {
                    await db.SaveChangesAsync();
                }

                
                if (Items.Count > 0)
                {
                    foreach (var task in Items.Where(t => t.Identifier == "Sesijas"))
                    {
                        var match = sessionTasks.FirstOrDefault(x => x.Id == task.Id);
                        if (match != null)
                        {
                            task.SessionsRequired = match.SessionsRequired;
                        }
                    }

                    InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception)
            {
                var errorMessage = "Neizdevās pārrēķināt sesiju skaitu!";
                SnackbarService.Add(errorMessage, Severity.Error);
            }
        }
    }
}

