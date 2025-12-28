using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.DTO;
using System;
using System.Timers;
using static MudBlazor.CategoryTypes;

namespace Pilens.Components.Pages
{
    public partial class Pomodoro
    {
        private static System.Timers.Timer? aTimer;
        private static System.Timers.Timer? pTimer;
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
        
        private int CurrSession { get; set; } = 0;
        string PomodoroReqError = "Šis ir obligāts atribūts!";

        PomodoroDTO pomodoroData = new();
        private readonly object _timerLock = new(); //lai taimeris netruprina atjaunoties kamēr iestata pauzi

        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

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
            await pomodoroForm.Validate();
            if (pomodoroForm.IsValid == false)
            {
                SnackbarService.Add("Lūdzu, ievadiet korektus datus pirms saglabāšanas.", Severity.Error);
                return;
            }
            string errorMessage = string.Empty; ;
            if (aTimer != null)
            {
                errorMessage = "Kļūda atrodot taimeri!";
                SnackbarService.Add(errorMessage, Severity.Error);
            }
            RemainingSeconds = pomodoroData.Minutes * 60;

            // Create a timer with a second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateTimer;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            StartBtnPressed = true;
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
        private void SetPause(int min) //koda implementācijas piemērs ņemts no https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-9.0  un https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-9.0 
        {
            RemainingSeconds = min * 60;
            pTimer = new System.Timers.Timer(1000);
            pTimer.Elapsed += UpdatePauseTimer;
            pTimer.AutoReset = true;
            pTimer.Enabled = true;
        }
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
                    if (aTimer == null)
                    {
                        string errorMessage = "Kļūda atrodot taimeri!";
                        SnackbarService.Add(errorMessage, Severity.Error);
                        return;
                    }
                    aTimer.Stop();
                    aTimer.Dispose();
                    aTimer = null;
                    CurrSession++;

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
                        RemainingSeconds = 0;
                        StartBtnPressed = false;
                        StopBtnPressed = false;
                        CurrSession = 0;
                        InvokeAsync(StateHasChanged);
                    }

                }
            }
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
                    if (pTimer == null)
                    {
                        string errorMessage = "Kļūda atrodot taimeri!";
                        SnackbarService.Add(errorMessage, Severity.Error);
                        return;
                    }
                    pTimer?.Stop();
                    pTimer?.Dispose();
                    pTimer = null;

                    DeactivateActivePause();
                    SetTimer();
                }
            }
        }
        // Funkcija, kas atiestata taimeri uz sākotnējo stāvokli
        private void RestartTimer()
        {
            if (!IsShortPause && !IsLongPause)
            {
                if (aTimer == null)
                {
                    ErrorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(ErrorMessage, Severity.Error);
                    return;
                }
                aTimer.Stop();
                aTimer.Dispose();
                aTimer = null;
            }
            else
            {
                if (pTimer == null)
                {
                    ErrorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(ErrorMessage, Severity.Error);
                    return;
                }
                pTimer.Stop();
                pTimer.Dispose();
                pTimer = null;
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
            if (pTimer == null)
            {
                string errorMessage = "Kļūda atrodot taimeri!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
            pTimer.Stop();
            pTimer.Dispose();
            pTimer = null;
            DeactivateActivePause();
            SetTimer();
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
                if (aTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                aTimer.Stop();
            }
            else
            {
                if (pTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                pTimer.Stop();
            }
            StopBtnPressed = true;
        }

        // Funkcija, kas turpina taimeri pēc apturēšanas
        private void ContinueTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            if (!IsShortPause && !IsLongPause)
            {
                if (aTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                aTimer.Start();
            }
            else
            {
                if (pTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                pTimer.Start();
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
