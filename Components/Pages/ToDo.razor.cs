using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.Models;

namespace Pilens.Components.Pages
{
    public partial class ToDo
    {
        //private readonly ApplicationDbContext _context;

        //public ToDo(ApplicationDbContext context)
        //{
        //    _context = context;
        //}
        public List<ToDoTask> Items { get; set; } = new List<ToDoTask>();

        private string? newTaskTitle = string.Empty;
        private string? newTaskDescription = string.Empty;

        public string newTaskEffort = "00:30";

        public DateTime newTaskDeadline = DateTime.Now;
        public bool isAdding = false;
        private ToDoTask? editingTask = null;
        // Aprēķinātais sesiju skaits izpildei, ja uzdevums ir "Sesijas" tipa
        public int SessionsRequired { get; set; } = 0;



        //Mudblazor nav DateTime komponentes, tādēļ sadalīju datumu un laiku
        public DateTime? newTaskDate { get; set; }
        public TimeSpan? newTaskTime { get; set; }

        // Pieprasītais laiks uzdevuma izpildei
        public int newTaskEffortHours { get; set; } = 0;
        public int newTaskEffortMinutes { get; set; } = 30;

        public IEnumerable<string> selectedGroupForNewTask { get; set; } = new HashSet<string> { "General" };

        public List<string> Groups { get; set; } = new List<string>() { "General" };

        public int newProgressTargetUnits { get; set; } = 0;        // Progresa mērķa vienības
        public int newProgressCurrentUnits { get; set; } = 0;       // Progresa esošās vienības
        public string? newProgressUnitType { get; set; } = "lpp";   // Vienības tips progresam (piem., lpp)

        private string? newGroupName { get; set; }
        private void ItemUpdated(MudItemDropInfo<ToDoTask> dropItem)
        {
            dropItem.Item.Identifier = dropItem.DropzoneIdentifier;

            // Aprēķina sesiju skaitu  no EffortDuration un apaļo uz augšu
            var minutes = (int)Math.Ceiling(dropItem.Item.EffortDuration.TotalMinutes);

            if (dropItem.DropzoneIdentifier == "Sesijas")
            {
                const int pomodoroMinutes = 25;
                dropItem.Item.SessionsRequired = minutes > 0
                    ? (int)Math.Ceiling(minutes / (double)pomodoroMinutes)
                    : 0;
            }
            else
            {
                dropItem.Item.SessionsRequired = 0;
            }

            InvokeAsync(StateHasChanged);
        }

        public int TotalSessions => Items.Where(i => i.Identifier == "Sesijas").Sum(i => i.SessionsRequired);

        void ToggleIsAdding()
        {
            // Toggle, bet atjaunot formu, kad rediģē
            isAdding = !isAdding;
            if (!isAdding)
                ResetForm();
        }

        private void AddOrUpdateTask()
        {
            if (string.IsNullOrWhiteSpace(newTaskTitle))
                return;

            var date = newTaskDate ?? DateTime.Today;
            var time = newTaskTime ?? TimeSpan.Zero;
            var combined = date.Date + time;
            newTaskDeadline = combined;

            if (!TimeSpan.TryParseExact(newTaskEffort, "hh\\:mm", null, out var effortSpan))
                return;

            var totalMinutes = (int)effortSpan.TotalMinutes;

            if (!string.IsNullOrWhiteSpace(newGroupName))
            {
                if (!Groups.Contains(newGroupName))
                    Groups.Add(newGroupName);

                var selected = selectedGroupForNewTask?.ToList() ?? new List<string>();
                if (!selected.Contains(newGroupName))
                    selected.Add(newGroupName);
                selectedGroupForNewTask = selected;
            }

            if (editingTask != null)
            {
                editingTask.Title = newTaskTitle;
                editingTask.Description = newTaskDescription ?? string.Empty;
                editingTask.Effort = totalMinutes;
                editingTask.Deadline = combined;
                editingTask.EffortDuration = effortSpan;
                editingTask.ProgressCurrentUnits = newProgressCurrentUnits;
                editingTask.ProgressTargetUnits = newProgressTargetUnits;
                editingTask.ProgressUnitType = newProgressUnitType;
                editingTask.Groups = selectedGroupForNewTask
                    .Select(name => new Group { Name = name })
                    .ToList();
                editingTask = null;
            }
            else
            {
                Items.Add(
                    new ToDoTask
                    {
                        Title = newTaskTitle ?? string.Empty,
                        Description = newTaskDescription ?? string.Empty,
                        Effort = totalMinutes,
                        Deadline = combined,
                        EffortDuration = effortSpan,
                        Identifier = "Saraksts",
                        Groups = selectedGroupForNewTask
                            .Select(name => new Group { Name = name })
                            .ToList(),
                        SessionsRequired = 0,
                        ProgressCurrentUnits = newProgressCurrentUnits,
                        ProgressTargetUnits = newProgressTargetUnits,
                        ProgressUnitType = newProgressUnitType
                    }
                );
            }

            ResetForm();
        }

        private void StartEdit(ToDoTask task)
        {
            if (task == null) return;
            editingTask = task;
            newTaskTitle = task.Title;
            newTaskDescription = task.Description;

            var totalMinutes = task.Effort > 0 ? task.Effort : (int)task.EffortDuration.TotalMinutes;
            newTaskEffort = $"{totalMinutes / 60:00}:{totalMinutes % 60:00}";
            newTaskDeadline = task.Deadline;
            newTaskDate = task.Deadline.Date;
            newTaskTime = task.Deadline.TimeOfDay;
            newProgressCurrentUnits = task.ProgressCurrentUnits;
            newProgressTargetUnits = task.ProgressTargetUnits;
            newProgressUnitType = task.ProgressUnitType;
            selectedGroupForNewTask = task.Groups?.Select(g => g.Name).ToList() ?? new List<string>();
            newGroupName = null;
            isAdding = true;
        }

        private void RemoveTask(ToDoTask task)
        {
            if (task == null) return;
            if (editingTask == task)
                ResetForm();

            Items.Remove(task);
        }

        private void CancelEdit() => ResetForm();

        private void ResetForm()
        {
            newTaskTitle = string.Empty;
            newTaskDescription = string.Empty;
            newTaskEffort = "00:30";
            newTaskDeadline = DateTime.Now;
            newTaskDate = null;
            newTaskTime = null;
            isAdding = false;
            newProgressUnitType = "lpp";
            newProgressTargetUnits = 0;
            newProgressCurrentUnits = 0;
            editingTask = null;
            selectedGroupForNewTask = new List<string>();
            newGroupName = null;
        }

        private bool TryParseEffortToMinutes(string? hhmm, out int minutes)
        {
            minutes = 0;
            if (string.IsNullOrWhiteSpace(hhmm)) return false;

            var parts = hhmm.Split(':');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out var h)) return false;
            if (!int.TryParse(parts[1], out var m)) return false;
            if (h < 0 || m < 0 || m > 59) return false;

            minutes = h * 60 + m;
            return true;
        }

       
        private static double GetProgressPercent(ToDoTask task)
        {
            if (task == null) return 0;
            if (task.ProgressTargetUnits <= 0) return 0;
            var percent = (double)task.ProgressCurrentUnits / task.ProgressTargetUnits * 100.0;
            if (percent < 0) return 0;
            if (percent > 100) return 100;
            return percent;
        }
    }
}

