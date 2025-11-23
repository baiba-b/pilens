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

        void ToggleExpanded()
        {
            expanded = true;
        }

        private void AddOrUpdateTask()
        {
            if (!string.IsNullOrWhiteSpace(newTaskTitle))
            {
                Items.Add(new ToDoTask
                {
                    Title = newTaskTitle,
                    Description = newTaskDescription,
                    Effort = newTaskEffort,
                    Deadline = newTaskDeadline
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
