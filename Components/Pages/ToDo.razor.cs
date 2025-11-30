namespace Pilens.Components.Pages
{
    public partial class ToDo
    {
        public List<ToDoTask> Items { get; set; } = new List<ToDoTask>();

        private string? newTaskTitle = string.Empty;
        private string? newTaskDescription = string.Empty;
        public int newTaskEffort = 2;
        public DateTime newTaskDeadline = DateTime.Now;
        public bool isAdding = false;
        private ToDoTask? editingTask = null;

        //Mudblazor nav DateTime komponentes, tādēļ sadalīju datumu un laiku
        public DateTime? newTaskDate { get; set; }
        public TimeSpan? newTaskTime { get; set; }

        // Pieprasītais laiks uzdevuma izpildei
        public int newTaskEffortHours { get; set; } = 0;
        public int newTaskEffortMinutes { get; set; } = 30;

        void ToggleIsAdding()
        {
            // Toggle, bet atjaunot formu, kad rediģē
            isAdding = !isAdding;
            if (!isAdding)
                ResetForm();
        }

        private void StartEdit(ToDoTask task)
        {
            if (task == null) return;
            editingTask = task;
            newTaskTitle = task.Title;
            newTaskDescription = task.Description;
            newTaskEffort = task.Effort;
            newTaskDeadline = task.Deadline;
            newTaskDate = task.Deadline.Date;
            newTaskTime = task.Deadline.TimeOfDay;
            newTaskEffortHours = task.EffortDuration.Hours + task.EffortDuration.Days * 24;
            newTaskEffortMinutes = task.EffortDuration.Minutes;
            isAdding = true;
        }

        private void AddOrUpdateTask()
        {
            if (string.IsNullOrWhiteSpace(newTaskTitle))
                return;

            var date = newTaskDate ?? DateTime.Today;
            var time = newTaskTime ?? TimeSpan.Zero;
            var combined = date.Date + time;

            newTaskDeadline = combined;

            var effortSpan = new TimeSpan(newTaskEffortHours, newTaskEffortMinutes, 0);

            if (editingTask != null)
            {
                // Rediģē 
                editingTask.Title = newTaskTitle;
                editingTask.Description = newTaskDescription ?? string.Empty;
                editingTask.Effort = newTaskEffort;
                editingTask.Deadline = combined;
                editingTask.EffortDuration = effortSpan;
                editingTask = null;
            }
            else
            {
                // Pievieno jaunu uzdevumu
                Items.Add(new ToDoTask
                {
                    Title = newTaskTitle,
                    Description = newTaskDescription,
                    Effort = newTaskEffort,
                    Deadline = combined,
                    EffortDuration = effortSpan
                });
            }

            ResetForm();
        }

        private void RemoveTask(ToDoTask task)
        {
            if (task == null) return;
            // atceļ redi
            if (editingTask == task)
                ResetForm();

            Items.Remove(task);
        }

        private void CancelEdit()
        {
            ResetForm();
        }

        private void ResetForm()
        {
            newTaskTitle = string.Empty;
            newTaskDescription = string.Empty;
            newTaskEffort = 2;
            newTaskDeadline = DateTime.Now;
            newTaskDate = null;
            newTaskTime = null;
            newTaskEffortHours = 0;
            newTaskEffortMinutes = 30;
            isAdding = false;
            editingTask = null;
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

    public TimeSpan EffortDuration { get; set; } = TimeSpan.Zero;
}
