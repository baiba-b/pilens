namespace Pilens.Components.Pages
{
    public partial class Home
    {
        private Pilens.Components.Pages.ToDo? toDoRef;
        private Pilens.Components.Pages.Pomodoro? pomodoroRef;

        private void StartAggregatedPomodoro()
        {
            var sessions = toDoRef?.TotalSessions ?? 0;
            pomodoroRef?.InitializeAndStartSessions(sessions);
        }
    }
}
