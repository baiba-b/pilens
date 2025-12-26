using Pilens.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Composition;

namespace Pilens.Data.DTO
{
    public class ToDoTaskDTO
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Nosaukums nevar būt garāks par 200 simboliem.")]
        public string Title { get; set; }
        public string? Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        [Required]
        [Range(int.MinValue, 3, ErrorMessage = "Vērtība nevar būt lielāka par 3.")]
        public int Effort { get; set; } = 1;
        [Required]
        public DateTime Deadline { get; set; } = DateTime.Now;
        [Required]
        public TimeSpan EffortDuration { get; set; } = TimeSpan.FromMinutes(30);
        [InverseProperty(nameof(ToDoTaskGroup.ToDoTask))]
        public ICollection<ToDoTaskGroup> ToDoTaskGroups { get; set; } = new List<ToDoTaskGroup>();

        public string Identifier { get; set; } = "Saraksts";

        public int SessionsRequired { get; set; } = 0;

        public int ProgressTargetUnits { get; set; } = 0;
        public int ProgressCurrentUnits { get; set; } = 0;
        public string ProgressUnitType { get; set; } = "lpp";
        public ToDoTaskDTO()
        {
        }
        public ToDoTaskDTO(ToDoTask task, IEnumerable<ToDoTaskGroup>? toDoTasksGroup)
        {
            this.Title = task.Title;
            this.Description = task.Description;
            this.IsCompleted = task.IsCompleted;
            this.Effort = task.Effort;
            this.Deadline = task.Deadline;
            this.EffortDuration = task.EffortDuration;
            this.ToDoTaskGroups = task.ToDoTaskGroups;
            this.Identifier = task.Identifier;
            this.SessionsRequired = task.SessionsRequired;
            this.ProgressCurrentUnits = task.ProgressCurrentUnits;
            this.ProgressTargetUnits = task.ProgressTargetUnits;
            this.ProgressUnitType = task.ProgressUnitType;
            this.ToDoTaskGroups = toDoTasksGroup != null ? toDoTasksGroup.ToList() : new List<ToDoTaskGroup>();
        }
    }
}
