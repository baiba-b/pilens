namespace Pilens.Components.Pages
{
    public partial class Home
    {
        private Pilens.Components.Pages.ToDo? toDoRef;
        private Pilens.Components.Pages.Pomodoro? pomodoroRef;
        public string errorMessage { get; set; } = string.Empty;

        private void StartAggregatedPomodoro()
        {
            var sessions = toDoRef?.TotalSessions ?? 0;
            if (sessions <= 0)
            {
                errorMessage = "Sesiju skaitam jābūt pozitīvam.";
                return;
            }

            errorMessage = string.Empty;

            pomodoroRef?.AddSessions(sessions);
        }
    }
}
