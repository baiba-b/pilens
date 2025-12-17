using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Timers;

namespace Pilens.Components.Pages
{
    public partial class Pomodoro
    {
        private static System.Timers.Timer? aTimer;
        private static System.Timers.Timer? pTimer;
        private int InputMinutes { get; set; } = 25; 
        private int InputPauseMinutes { get; set; } = 5;
        private int InputLongPauseMinutes { get; set; } = 20; //vajadzēs iespēju skippot pauzi + 4 sesijai noņemt īso pauzi
        private int InputSessionAmount { get; set; } = 4; //  (viena sesija = 1 pomodoro + pauze)
        private int InputSessionLongPause { get; set; } = 4;
        private int RemainingSeconds { get; set; } = 0;
        private string DisplayStatus { get; set; } = "Stop";
        private string? ErrorMessage { get; set; }
        private bool StartBtnPressed { get; set; } = false;
        private bool StopBtnPressed  { get; set; } = false;
        private bool IsShortPause { get; set; } = false;
        private bool IsLongPause { get; set; } = false;
        private int AdjustedMin { get; set; } = 5;
        private bool isDone { get; set; } = false;


        private int CurrSession { get; set; } = 0;
        private readonly object _timerLock = new(); //lai taimeris netruprina atjaunoties kamēr iestata pauzi
        private string DisplayTime =>
            TimeSpan.FromSeconds(RemainingSeconds).ToString(@"mm\:ss");

        public bool IsRunning => StartBtnPressed;

       
        public void AddSessions(int sessions)
        {
            if (sessions <= 0) return;

            if (StartBtnPressed)
            {
                InputSessionAmount += sessions;
                InvokeAsync(StateHasChanged);
            }
            else
            {
                InitializeAndStartSessions(sessions);
            }
        }

        // Funkcija, kas izveido  un sāk taimeri
        private void SetTimer() //System.Timer funkciju implementācijas piemērs & apraksts ņemts no https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-9.0  un https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-9.0 
        {
            if (aTimer != null) return;
            RemainingSeconds = InputMinutes * 60;
            ErrorMessage = string.Empty;
            if (RemainingSeconds <= 0)
            {
                ErrorMessage = "Lūdzu ievadi pozitīvu minūšu skaitu.";
                return;
            }
           
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateTimer;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            StartBtnPressed = true;
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
                    aTimer.Stop();
                    aTimer.Dispose();
                    aTimer = null;
                    CurrSession++;

                    if (CurrSession < InputSessionAmount && CurrSession % InputSessionLongPause != 0)
                    {
                        IsShortPause = true;
                        SetPause(InputPauseMinutes);
                    }
                    else if (CurrSession < InputSessionAmount)
                    {
                        IsLongPause = true;
                        SetPause(InputLongPauseMinutes);
                    }
                    else
                    {                         //Restartē visu pēc visu sesiju pabeigšanas
                        

                        InputMinutes = InputMinutes;
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
                aTimer.Stop();
                aTimer.Dispose();
                aTimer = null;
            }
            else
            {
                pTimer.Stop();
                pTimer.Dispose();
                pTimer = null;
            }

            DeactivateActivePause();

            InputMinutes = InputMinutes;
            RemainingSeconds = 0;
            StartBtnPressed = false;
            StopBtnPressed = false;
            CurrSession = 0;
            InvokeAsync(StateHasChanged);
        }

        private void SkipPause(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
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
                
           if(!IsShortPause && !IsLongPause) aTimer.Stop();
           else pTimer.Stop();
           StopBtnPressed = true;
        }
      
        // Funkcija, kas turpina taimeri pēc apturēšanas
        private void ContinueTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            if (!IsShortPause && !IsLongPause) aTimer.Start();
            else pTimer.Start();
            StopBtnPressed = false;
            InvokeAsync(StateHasChanged);
        }
        private void AdjustTime(bool adjustType)
        {
            if (adjustType == true)
            {
                RemainingSeconds += AdjustedMin*60;
            }
            else
            {
                if (RemainingSeconds > AdjustedMin * 60) RemainingSeconds -= AdjustedMin * 60;
                else RemainingSeconds = 0;
            }
                
        
        }
 
        public void InitializeAndStartSessions(int sessions)
        {
            InputSessionAmount = sessions;
            CurrSession = 0;
            isDone = false;
            StartBtnPressed = true;
            SetTimer();
        }
    }
}
