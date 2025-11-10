using System;
using System.Timers;

namespace Pilens.Components.Pages
{
    public partial class Pomodoro
    {
        private static System.Timers.Timer aTimer;
        private static System.Timers.Timer pTimer;
        private int InputMinutes { get; set; } = 25;
        private int InputPauseMinutes { get; set; } = 5;
        private int InputLongPauseMinutes { get; set; } = 20; //vajadzēs iespēju skippot pauzi + 4 sesijai noņemt īso pauzi
        private int InputSessionAmount { get; set; } = 4; //  (viena sesija = 1 pomodoro + pauze)

        private int InputSessionLongPause { get; set; } = 4;
        private int RemainingSeconds { get; set; } = 0;
        private string DisplayStatus { get; set; } = "Stop";
        private string ErrorMessage { get; set; }
        private bool StartBtnPressed { get; set; } = false;
        private bool StopBtnPressed  { get; set; } = false;

        private bool IsPause { get; set; } = false;
        private int CurrSession { get; set; } = 0;
        private string DisplayTime =>
            TimeSpan.FromSeconds(RemainingSeconds).ToString(@"mm\:ss");

        private void SetTimer() //koda implementācijas piemērs ņemts no https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-9.0  un https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-9.0 
        {
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
        private void SetPause(int min) //koda implementācijas piemērs ņemts no https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-9.0  un https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-9.0 
        {
            RemainingSeconds = min * 60;

            pTimer = new System.Timers.Timer(1000);
            pTimer.Elapsed += UpdatePauseTimer;
            pTimer.AutoReset = true;
            pTimer.Enabled = true;
            StartBtnPressed = true;
        }
        private void UpdatePauseTimer(Object source, ElapsedEventArgs e)
        {
            if (RemainingSeconds > 0)
            {
                RemainingSeconds--;
                InvokeAsync(StateHasChanged);
            }
            else
            {
                IsPause = false;
                SetTimer();
            }
        }

        private void StopTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
                
           if(!IsPause) aTimer.Stop();
           else pTimer.Stop();
            DisplayStatus = "Continue";
            StopBtnPressed = true;
        }
        private void UpdateTimer(Object source, ElapsedEventArgs e)
        {
            if (RemainingSeconds > 0)
            {
                RemainingSeconds--;
                InvokeAsync(StateHasChanged);
            }
            else
            {
                CurrSession++;
                IsPause = true;
                if (CurrSession < InputSessionAmount && CurrSession < 4) SetPause(InputPauseMinutes);
                else SetPause(InputLongPauseMinutes);
                aTimer.Stop();
                aTimer.Dispose();
            }
        }
        private void ResetTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            if (!IsPause)
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
            IsPause = false;
             

            InputMinutes = 25;
            RemainingSeconds = 0;
            StartBtnPressed = false;
            StopBtnPressed = false;
            CurrSession = 0;
            InvokeAsync(StateHasChanged);
        }
        private void ContinueTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            if (!IsPause) aTimer.Start();
            else pTimer.Start();
            StopBtnPressed = false;
            InvokeAsync(StateHasChanged);
        }
    }
}
