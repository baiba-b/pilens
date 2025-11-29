namespace Pilens.Components.Pages
{
    public partial class ToDo
    {
        public List<ToDoTask> Items { get; set; } = new List<ToDoTask>();

        private string? newTaskTitle = string.Empty;
        private string? newTaskDescription = string.Empty;
        public int newTaskEffort = 2;
        public DateTime newTaskDeadline = DateTime.Now;
        public bool expanded = false;
        private ToDoTask? editingTask = null;

        //Mudblazor nav DateTime komponentes, tādēļ sadalīju datumu un laiku
        public DateTime? newTaskDate { get; set; }
        public TimeSpan? newTaskTime { get; set; }

        void ToggleExpanded()
        {
            expanded = true;
        }

        private void AddOrUpdateTask()
        {
            if (!string.IsNullOrWhiteSpace(newTaskTitle))
            {
                var date = newTaskDate ?? DateTime.Today;
                var time = newTaskTime ?? TimeSpan.Zero;
                var combined = date.Date + time;

                newTaskDeadline = combined;

                Items.Add(new ToDoTask
                {
                    Title = newTaskTitle,
                    Description = newTaskDescription,
                    Effort = newTaskEffort,
                    Deadline = combined
                });

                ResetForm();
            }
        }

        private void ResetForm()
        {
            newTaskTitle = string.Empty;
            newTaskDescription = string.Empty;
            newTaskEffort = 2;
            newTaskDeadline = DateTime.Now;
            newTaskDate = null;
            newTaskTime = null;
            expanded = false;
        }
    }
}

public partial class ToDoTask
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public int Effort { get; set; } = 2;
    public DateTime Deadline { get; set; } = DateTime.Now;
}
