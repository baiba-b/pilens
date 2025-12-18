using Pilens.Components.Pages.todo;
using System.Threading.Tasks;

namespace Pilens.Components.Pages
{
    public partial class Home
    {
        public const string SessionsMustBePositiveMessage = "Sesiju skaitam jābūt pozitīvam.";
        public const string PomodoroUnavailableMessage = "Pomodoro modulis nav pieejams.";

        private ToDo? toDoRef;
        private Pilens.Components.Pages.Pomodoro? pomodoroRef;
        private Pilens.Components.Pages.Console? consoleRef;
        public string errorMessage { get; set; } = string.Empty;

        private async Task HandleStartPomodoro(int sessions)
        {
            if (sessions <= 0)
            {
                errorMessage = Home.SessionsMustBePositiveMessage;
                await InvokeAsync(StateHasChanged);
                return;
            }

            if (pomodoroRef == null)
            {
                errorMessage = Home.PomodoroUnavailableMessage;
                await InvokeAsync(StateHasChanged);
                return;
            }

            errorMessage = string.Empty;
            pomodoroRef.AddSessions(sessions);
            await InvokeAsync(StateHasChanged);
        }
    }
}
