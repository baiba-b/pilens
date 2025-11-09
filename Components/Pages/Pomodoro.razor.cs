using System;
using System.Timers;

namespace Pilens.Components.Pages
{
    public partial class Pomodoro
    {
        private static System.Timers.Timer aTimer;
        private int InputMinutes { get; set; } = 25;
        private int InputPauseMinutes { get; set; } = 5;
        private int InputLongPauseMinutes { get; set; } = 20; //vajadzēs iespēju skippot pauzi + 4 sesijai noņemt īso pauzi
        private int InputSessionAmount { get; set; } = 4; //  (viena sesija = 1 pomodoro + pauze)
        private int RemainingSeconds { get; set; } = 0;
        private string DisplayStatus { get; set; } = "Stop";
        private string ErrorMessage { get; set; }
        private bool StartBtnPressed { get; set; } = false;
        private bool StopBtnPressed  { get; set; } = false;
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

        private void StopTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            //if(DisplayStatus == "Stop")
            //{
                aTimer.Stop();
                DisplayStatus = "Continue";
                StopBtnPressed = true;
            //}
            //else
            //{
            //    aTimer.Start();
            //    DisplayStatus = "Stop";
            //}
            //StateHasChanged();

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
                aTimer.Stop();
                aTimer.Dispose();
            }
        }
        private void ResetTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            aTimer.Stop();
            aTimer.Dispose();
            aTimer = null;
            InputMinutes = 25;
            RemainingSeconds = 0;
            StartBtnPressed = false;
            StopBtnPressed = false;
            DisplayStatus = "Stop";
            InvokeAsync(StateHasChanged);
        }
        private void ContinueTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            aTimer.Start();
            StopBtnPressed = false;
            InvokeAsync(StateHasChanged);
        }
    }
}
