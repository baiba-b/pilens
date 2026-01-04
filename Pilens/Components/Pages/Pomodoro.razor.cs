using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.DTO;
using Pilens.Data.State;
using System;
using System.Linq;
using System.Timers;
using static MudBlazor.CategoryTypes;
using Microsoft.JSInterop;

namespace Pilens.Components.Pages
{
    public partial class Pomodoro
    {
        private System.Timers.Timer? productiveTimer;
        private System.Timers.Timer? pauseTimer;
        private int RemainingSeconds { get; set; } = 0;
        private string DisplayStatus { get; set; } = "Stop";
        private bool StartBtnPressed { get; set; } = false;
        private bool StopBtnPressed { get; set; } = false;
        private bool IsShortPause { get; set; } = false;
        private bool IsLongPause { get; set; } = false;
        string ErrorMessage = string.Empty;
        private bool isDone { get; set; } = false;
        bool success;
        private MudForm? pomodoroForm;
        private bool isInitialized = false;

        private int CurrSession { get; set; } = 0;
        string PomodoroReqError = "Šis ir obligāts atribūts!";

        PomodoroDTO pomodoroData = new();
        private readonly object _timerLock = new(); //lai taimeris netruprina atjaunoties kamēr iestata pauzi

        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

        [Inject]
        private PomodoroState PomodoroState { get; set; } = default!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        private string DisplayTime =>
            TimeSpan.FromSeconds(RemainingSeconds).ToString(@"mm\:ss");

        public bool IsRunning => StartBtnPressed;

       
        private string PomodoroValidation(int arg)
        {
            string errorMessage = string.Empty;

            if (arg < 1)
            {
                errorMessage = "Lūdzu ievadi pozitīvu skaitli.";
                return errorMessage;
            }
            else return errorMessage;
        }

        protected override async Task OnInitializedAsync()
        {
            success = true;
            var userId = await getUserId();
            if (userId == null)
            {
                return;
            }

            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var existingPomodoro = await db.Pomodoros
               .AsNoTracking()
               .FirstOrDefaultAsync(p => p.UserID == userId);
                if (existingPomodoro is not null)
                {
                    pomodoroData = new PomodoroDTO(existingPomodoro);
                    pomodoroData.UserID = userId;
                    RemainingSeconds = pomodoroData.Minutes * 60;
                    PomodoroState.Minutes = pomodoroData.Minutes;
                }
                else
                {
                    PomodoroState.Minutes = pomodoroData.Minutes;
                }
            }
            catch (Exception)
            {
                string errorMessage = "Kļūda ielādējot datus.";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }

        }
        /// <summary>
        /// Funkcija, kas izveido un sāk taimeri
        /// </summary>
        private async Task SetTimer() //System.Timer funkciju implementācijas piemērs & apraksts ņemts no https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-9.0  un https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-9.0 
        {
            if (isInitialized == false)
            {
                await pomodoroForm.Validate();
                if (pomodoroForm.IsValid == false)
                {
                    SnackbarService.Add("Lūdzu, ievadiet korektus datus pirms saglabāšanas.", Severity.Error);
                    return;
                }
            }

            PomodoroState.Minutes = pomodoroData.Minutes;

            string errorMessage = string.Empty;
            if (productiveTimer != null)
            {
                errorMessage = "Kļūda izveidojot taimeri!";
                SnackbarService.Add(errorMessage, Severity.Error);
                productiveTimer = null;
            }
            RemainingSeconds = pomodoroData.Minutes * 60;

            // Create a timer with a second interval.
            
            productiveTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            productiveTimer.Elapsed += UpdateTimer;
            productiveTimer.AutoReset = true;
            productiveTimer.Enabled = true;
            StartBtnPressed = true;
            isInitialized = true; 
        }
        public void AddSessions(int sessions)
        {
            if (sessions <= 0)
            {
                return;
            }

            if (StartBtnPressed)
            {
                pomodoroData.SessionAmount += sessions;
                InvokeAsync(StateHasChanged);
            }
            else
            {
                InitializeAndStartSessions(sessions);
            }
        }
        // Funkcija, kas sāk pauzes taimeri
      
        // Funkcija, kas atjauno UI un atlikošu laiku atbilstoši darba taimerim
        private void UpdateTimer(object source, ElapsedEventArgs e)
        {
            lock (_timerLock)
            {
                if (RemainingSeconds > 0)
                {
                    RemainingSeconds--;
                    InvokeAsync(StateHasChanged);
                }
                else
                {
                    if (productiveTimer == null)
                    {
                        string errorMessage = "Kļūda atrodot taimeri!";
                        SnackbarService.Add(errorMessage, Severity.Error);
                        return;
                    }
                    productiveTimer.Stop();
                    productiveTimer.Dispose();
                    productiveTimer = null;
                    CurrSession++;

                    _ = InvokeAsync(() => PlaySoundAsync("yippe"));

                    if (CurrSession < pomodoroData.SessionAmount && CurrSession % pomodoroData.SessionLongPause != 0)
                    {
                        IsShortPause = true;
                        SetPause(pomodoroData.PauseMinutes);
                    }
                    else if (CurrSession < pomodoroData.SessionAmount)
                    {
                        IsLongPause = true;
                        SetPause(pomodoroData.LongPauseMinutes);
                    }
                    else
                    {   //Restartē visu pēc visu sesiju pabeigšanas
                        isInitialized = false;
                        RemainingSeconds = 0;
                        StartBtnPressed = false;
                        StopBtnPressed = false;
                        CurrSession = 0;
                        InvokeAsync(StateHasChanged);
                    }

                }
            }
        }

        private void SetPause(int min) //koda implementācijas piemērs ņemts no https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-9.0  un https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-9.0 
        {
            RemainingSeconds = min * 60;
            pauseTimer = new System.Timers.Timer(1000);
            pauseTimer.Elapsed += UpdatePauseTimer;
            pauseTimer.AutoReset = true;
            pauseTimer.Enabled = true;
        }
        // Funkcija, kas atjauno UI un atlikošu laiku atbilstoši pauzes taimerim 
        private void UpdatePauseTimer(object source, ElapsedEventArgs e)
        {
            lock (_timerLock)
            {
                if (RemainingSeconds > 0)
                {
                    RemainingSeconds--;
                    InvokeAsync(StateHasChanged);
                }
                else
                {
                    if (pauseTimer == null)
                    {
                        string errorMessage = "Kļūda atrodot taimeri!";
                        SnackbarService.Add(errorMessage, Severity.Error);
                        return;
                    }
                    pauseTimer?.Stop();
                    pauseTimer?.Dispose();
                    pauseTimer = null;

                    _ = InvokeAsync(() => PlaySoundAsync("item-pick-up"));

                    DeactivateActivePause();
                    SetTimer();
                }
            }
        }
        // Funkcija, kas atiestata taimeri uz sākotnējo stāvokli
        private void StopPomodoroTimer()
        {
            if (!IsShortPause && !IsLongPause)
            {
                if (productiveTimer == null)
                {
                    ErrorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(ErrorMessage, Severity.Error);
                    return;
                }
                productiveTimer.Stop();
                productiveTimer.Dispose();
                productiveTimer = null;
                isInitialized = false;
            }
            else
            {
                if (pauseTimer == null)
                {
                    ErrorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(ErrorMessage, Severity.Error);
                    return;
                }
                pauseTimer.Stop();
                pauseTimer.Dispose();
                isInitialized = false;
                pauseTimer = null;
            }

            DeactivateActivePause();

            RemainingSeconds = 0;
            StartBtnPressed = false;
            StopBtnPressed = false;
            CurrSession = 0;
            InvokeAsync(StateHasChanged);
        }
        private void SkipPause(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            if (pauseTimer == null)
            {
                string errorMessage = "Kļūda atrodot taimeri!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
            pauseTimer.Stop();
            pauseTimer.Dispose();
            pauseTimer = null;
            DeactivateActivePause();
            _ = PlaySoundAsync("item-pick-up");
            SetTimer();
        }
        // Kods ģenerēts ar AI rīku
        private Task PlaySoundAsync(string clipName)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                return Task.CompletedTask;
            }

            return JSRuntime.InvokeVoidAsync("audioController.playSound", clipName).AsTask();
        }

        private void DeactivateActivePause()
        {
            if (IsShortPause)
            {
                IsShortPause = false;
                return;
            }

            if (IsLongPause)
            {
                IsLongPause = false;
            }
        }

        //Funkcija, kas aptur taimeri
        private void PauseTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {

            if (!IsShortPause && !IsLongPause)
            {
                if (productiveTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                productiveTimer.Stop();
            }
            else
            {
                if (pauseTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                pauseTimer.Stop();
            }
            StopBtnPressed = true;
        }

        // Funkcija, kas turpina taimeri pēc apturēšanas
        private void ContinueTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            if (!IsShortPause && !IsLongPause)
            {
                if (productiveTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                productiveTimer.Start();
            }
            else
            {
                if (pauseTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                pauseTimer.Start();
            }
            StopBtnPressed = false;
            InvokeAsync(StateHasChanged);
        }
        private void AdjustTime(bool adjustType)
        {
            if (adjustType == true)
            {
                RemainingSeconds += pomodoroData.AdjustedMin * 60;
            }
            else
            {
                if (RemainingSeconds > pomodoroData.AdjustedMin * 60) RemainingSeconds -= pomodoroData.AdjustedMin * 60;
                else RemainingSeconds = 0;
            }
        }
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

        public void InitializeAndStartSessions(int sessions)
        {
            pomodoroData.SessionAmount = sessions;
            CurrSession = 0;
            isDone = false;
            StartBtnPressed = true;
            SetTimer();
        }
        //TODO: Pārskatīt kļūdu apstrādi
        private async Task SavePomodoroData(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            await pomodoroForm.Validate();
            if(pomodoroForm.IsValid == false)
            {
                SnackbarService.Add("Lūdzu, ievadiet korektus datus pirms saglabāšanas.", Severity.Error);
                return;
            }
            var userId = await getUserId();
            if (userId == null)
            {
                string errorMessage = "Neizdevās identificēt lietotāju.";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
            pomodoroData.UserID = userId;
            PomodoroState.Minutes = pomodoroData.Minutes;

            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var existingPomodoro = await db.Pomodoros
                    .FirstOrDefaultAsync(p => p.UserID == userId);

                if (existingPomodoro is not null)
                {
                    existingPomodoro.Minutes = pomodoroData.Minutes;
                    existingPomodoro.PauseMinutes = pomodoroData.PauseMinutes;
                    existingPomodoro.LongPauseMinutes = pomodoroData.LongPauseMinutes;
                    existingPomodoro.SessionAmount = pomodoroData.SessionAmount;
                    existingPomodoro.SessionLongPause = pomodoroData.SessionLongPause;
                    existingPomodoro.AdjustedMin = pomodoroData.AdjustedMin;
                    db.Pomodoros.Update(existingPomodoro);
                    pomodoroData.updatePomodoro(existingPomodoro, pomodoroData);
                }
                else
                {
                    db.Pomodoros.Add(new Data.Models.Pomodoro
                    {
                        UserID = userId,
                        Minutes = pomodoroData.Minutes,
                        PauseMinutes = pomodoroData.PauseMinutes,
                        LongPauseMinutes = pomodoroData.LongPauseMinutes,
                        SessionAmount = pomodoroData.SessionAmount,
                        SessionLongPause = pomodoroData.SessionLongPause,
                        AdjustedMin = pomodoroData.AdjustedMin
                    });
                }

                await db.SaveChangesAsync();
                SnackbarService.Add("Dati saglabāti veiksmīgi!", Severity.Success);
            }
            catch (Exception)
            {
                SnackbarService.Add("Kļūda saglabājot datus!", Severity.Error);
            }
        }
    }
}
