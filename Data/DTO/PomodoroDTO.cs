namespace Pilens.Data.DTO
{
    public class PomodoroDTO
    {
        public string UserID { get; set; } = string.Empty;

        public int DurationInMinutes { get; set; }

        public int Minutes { get; set; } = 25;
        public int PauseMinutes { get; set; } = 5;
        public int LongPauseMinutes { get; set; } = 20;
        public int SessionAmount { get; set; } = 4;
        public int SessionLongPause { get; set; } = 4;
        public int AdjustedMin { get; set; } = 5;

        public virtual ApplicationUser User { get; set; } = null!;

        public PomodoroDTO() { }

        public PomodoroDTO(ApplicationUser user, int durationInMinutes, int minutes = 25, int pauseMinutes = 5,
            int longPauseMinutes = 20, int sessionAmount = 4, int sessionLongPause = 4, int adjustedMin = 5)
        {
            User = user;
            UserID = user.Id;
            DurationInMinutes = durationInMinutes;
            Minutes = minutes;
            PauseMinutes = pauseMinutes;
            LongPauseMinutes = longPauseMinutes;
            SessionAmount = sessionAmount;
            SessionLongPause = sessionLongPause;
            AdjustedMin = adjustedMin;
        }
    }
}
