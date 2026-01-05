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

        // Lietotāja uzdevumu saraksts
        private List<ToDoTask> Items { get; set; } = new();
        // Kļūdu ziņa UI
        private string? ErrorMessage { get; set; }
        private string NewGroupName { get; set; } = string.Empty;
        private string userId { get; set; }

        // Pašreizējais Pomodoro sesijas ilgums minūtēs
        private int pomodoroMinutes = 25;

        // Kopējais sesiju skaits no "Sesijas" dropzonas
        private int TotalSessions => Items
            .Where(task => task.Identifier == "Sesijas")
            .Sum(task => task.SessionsRequired);

        // Pārbauda, vai var startēt Pomodoro (ir vismaz viena sesija)
        private bool CanStartPomodoro => TotalSessions > 0;

        /// <summary>
        /// Inicializē ToDo komponenti, kas seko līdzi Pomodoro ilguma izmaiņām.
        /// </summary>
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

        /// <summary>
        /// Ielādē lietotāja uzdevumus no datubāzes un atjauno lokālo sarakstu.
        /// </summary>
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

        /// <summary>
        /// Aprēķina uzdevuma progresa procentus, balstoties uz mērķa vienībām.
        /// </summary>
        /// <param name="task">Uzdevums, kura progress jānovērtē</param>
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

            // Aprēķina procentuālo daļu un ierobežo robežās 0-100
            return Math.Clamp((double)task.ProgressCurrentUnits / task.ProgressTargetUnits * 100.0, 0, 100);
        }

        /// <summary>
        /// Sinhronizē Pomodoro minūtes no stāvokļa un pārrēķina sesiju skaitu.
        /// </summary>
        private void OnPomodoroMinutesChanged()
        {
            pomodoroMinutes = PomodoroState.Minutes;
            // '_' tiek izmantots kā discard, lai ignorētu atgriezto Task
            _ = InvokeAsync(RecalculateSessionsAsync);
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Apstrādā uzdevuma pārvietošanu starp dropzonām un atjaunina datubāzē.
        /// </summary>
        /// <param name="dropItem">Pārvietotais uzdevums un jaunās zonas identifikators</param>
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

                // Uzstāda jauno dropzonas identifikatoru
                entity.Identifier = dropItem.DropzoneIdentifier;

                var minutes = (int)Math.Ceiling(entity.EffortDuration.TotalMinutes);
                // Ja uzdevums tiek pārvietots uz "Sesijas" zonu, aprēķina nepieciešamās sesijas
                if (dropItem.DropzoneIdentifier == "Sesijas")
                {
                    var pomodoroLength = pomodoroMinutes > 0 ? pomodoroMinutes : 25;
                    entity.SessionsRequired = minutes > 0
                        ? (int)Math.Ceiling(minutes / (double)pomodoroLength)
                        : 0;
                }
                else
                {
                    // Citās zonās nesijas nav nepieciešamas
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

        /// <summary>
        /// Sāk Pomodoro taimeri no Uzdevumu moduļa, pārbaudot vai sesiju skaits ir pozitīvs.
        /// </summary>
        private async Task StartPomodoroFromDropzoneAsync()
        {
            if (!CanStartPomodoro)
            {
                ErrorMessage = SessionsMustBePositiveMessage;
                return;
            }

            ErrorMessage = null;

            // Izsauc callback funkciju, lai sāktu Pomodoro taimeri
            if (OnStartPomodoro.HasDelegate)
            {
                await OnStartPomodoro.InvokeAsync(TotalSessions);
            }
        }

        /// <summary>
        /// Pārslēdz uzdevuma pabeigtības statusu un saglabā izmaiņas.
        /// </summary>
        /// <param name="task">Uzdevums, kura statuss jāpārslēdz</param>
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

                // Pārslēdz IsCompleted vērtību uz pretējo
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

        /// <summary>
        /// Atver uzdevuma rediģēšanas lapu.
        /// </summary>
        /// <param name="task">Rediģējamais uzdevums</param>
        private void StartEdit(ToDoTask task)
        {
            Navigation.NavigateTo($"/ToDo/update/{task.Id}");
        }

        /// <summary>
        /// Pārbauda, vai uzdevumam var palielināt progresu.
        /// </summary>
        /// <param name="task">Uzdevums, kuru validē</param>
        private bool CanIncreaseProgress(ToDoTask task)
        {
            return task?.ProgressTargetUnits > 0 && task.ProgressCurrentUnits < task.ProgressTargetUnits;
        }

        /// <summary>
        /// Pārbauda, vai uzdevumam var samazināt progresu.
        /// </summary>
        /// <param name="task">Uzdevums, kuru validē</param>
        private bool CanDecreaseProgress(ToDoTask task)
        {
            return task?.ProgressTargetUnits > 0 && task.ProgressCurrentUnits > 0;
        }

        /// <summary>
        /// Maina uzdevuma progresu par norādīto vienību skaitu un saglabā datubāzē.
        /// </summary>
        /// <param name="task">Uzdevums, kura progress jāatjaunina</param>
        /// <param name="changeAmount">Vienību skaits, ko pieskaitīt vai atņemt</param>
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

                // Atjaunina progresa vērtību
                entity.ProgressCurrentUnits = newValue;
                await db.SaveChangesAsync();

                // Atjaunina arī lokālo objektu, lai UI atspoguļotu izmaiņas
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
            // Konfigurē apstiprinājuma dialoga parametrus
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

            // Ja lietotājs atcēla dialogu, neko nedara
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

                // Dzēš visas saistītās grupas
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

        /// <summary>
        /// Izveido jaunu grupu pēc ievadītā nosaukuma un validē unikālumu.
        /// </summary>
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
                // Pārbauda, vai grupa ar šādu nosaukumu jau eksistē
                var exists = await db.Groups.AnyAsync(group => group.Name == trimmedName);
                if (exists)
                {
                    ErrorMessage = "Grupa ar šādu nosaukumu jau pastāv!";
                    return;
                }

                db.Groups.Add(new Group { Name = trimmedName });
                await db.SaveChangesAsync();

                // Notīra ievades lauku pēc veiksmīgas izveides
                NewGroupName = string.Empty;
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Grupu nevarēja izveidot!";
            }
        }

        /// <summary>
        /// Nolasa lietotāja Id no autentikācijas konteksta.
        /// </summary>
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

        /// <summary>
        /// Navigē uz jauna uzdevuma izveides lapu.
        /// </summary>
        private void NavigateToCreate()
        {
            Navigation.NavigateTo($"ToDo/create");
        }

        /// <summary>
        /// Atvieno notikumu apstrādātāju, atbrīvojot resursus.
        /// </summary>
        public void Dispose()
        {
            PomodoroState.OnChange -= OnPomodoroMinutesChanged;
        }

        /// <summary>
        /// Pārrēķina nepieciešamo sesiju skaitu, balstoties uz aktuālo Pomodoro ilgumu.
        /// </summary>
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
                // Iegūst tikai uzdevumus, kas atrodas "Sesijas" zonā
                var sessionTasks = await db.ToDoTasks
                    .Where(t => t.UserID == userId && t.Identifier == "Sesijas")
                    .ToListAsync();

                var updated = false;
                foreach (var task in sessionTasks)
                {
                    var minutes = (int)Math.Ceiling(task.EffortDuration.TotalMinutes);
                    // Aprēķina jaunās sesijas, balstoties uz jaunajām Pomodoro minūtēm
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

                
                // Atjaunina arī lokālo Items sarakstu
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

