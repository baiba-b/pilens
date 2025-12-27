using Pilens.Components.Pages;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pilens.Data.DTO
{
    public class PomodoroDTO
    {
        public int Id { get; set; }
        public int DurationInMinutes { get; set; }

        public int Minutes { get; set; } = 25;
        public int PauseMinutes { get; set; } = 5;
        public int LongPauseMinutes { get; set; } = 20;
        public int SessionAmount { get; set; } = 4;
        public int SessionLongPause { get; set; } = 4;
        public int AdjustedMin { get; set; } = 5;

        public string UserID { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }

        public PomodoroDTO() { }

        public PomodoroDTO(ApplicationUser user, int durationInMinutes, int minutes = 25, int pauseMinutes = 5,
            int longPauseMinutes = 20, int sessionAmount = 4, int sessionLongPause = 4, int adjustedMin = 5)
        {
            UserID = user.Id;
            DurationInMinutes = durationInMinutes;
            Minutes = minutes;
            PauseMinutes = pauseMinutes;
            LongPauseMinutes = longPauseMinutes;
            SessionAmount = sessionAmount;
            SessionLongPause = sessionLongPause;
            AdjustedMin = adjustedMin;
        }

        public PomodoroDTO(Data.Models.Pomodoro pomodoro)
        {
            UserID = pomodoro.UserID;
            DurationInMinutes = pomodoro.Minutes;
            Minutes = pomodoro.Minutes;
            PauseMinutes = pomodoro.PauseMinutes;
            LongPauseMinutes = pomodoro.LongPauseMinutes;
            SessionAmount = pomodoro.SessionAmount;
            SessionLongPause = pomodoro.SessionLongPause;
            AdjustedMin = pomodoro.AdjustedMin;
        }
        public void updatePomodoro(Data.Models.Pomodoro pomodoro, PomodoroDTO pomodoroDTO)
        {
            pomodoroDTO.Minutes = pomodoro.Minutes;
            pomodoroDTO.PauseMinutes = pomodoro.PauseMinutes;
            pomodoroDTO.LongPauseMinutes = pomodoro.LongPauseMinutes;
            pomodoroDTO.SessionAmount = pomodoro.SessionAmount;
            pomodoroDTO.SessionLongPause = pomodoro.SessionLongPause;
            pomodoroDTO.AdjustedMin = pomodoro.AdjustedMin;

        }
    }
}
