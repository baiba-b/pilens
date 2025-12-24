using System.ComponentModel.DataAnnotations;
namespace Pilens.Data.Models;

public partial class ToDoTask
{
    [Key]
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
    [Required]  
    public ICollection<Group> Groups { get; set; } = new List<Group>();

    public string Identifier { get; set; } = "Saraksts";

    public int SessionsRequired { get; set; } = 0;

    public int ProgressTargetUnits { get; set; } = 0;
    public int ProgressCurrentUnits { get; set; } = 0;
    public string ProgressUnitType { get; set; } = "lpp";

    public ToDoTask()
    {
    }

    public ToDoTask(
        string title,
        string? description,
        bool isCompleted,
        int effort,
        DateTime deadline,
        TimeSpan effortDuration,
        ICollection<Group> groups,
        string identifier,
        int sessionsRequired,
        int progressTargetUnits,
        int progressCurrentUnits,
        string progressUnitType)
    {
        Title = title ?? string.Empty;
        Description = description;
        IsCompleted = isCompleted;
        Effort = effort;
        Deadline = deadline;
        EffortDuration = effortDuration;
        Groups = groups ?? new List<Group>();
        Identifier = identifier ?? string.Empty;
        SessionsRequired = sessionsRequired;
        ProgressTargetUnits = progressTargetUnits;
        ProgressCurrentUnits = progressCurrentUnits;
        ProgressUnitType = progressUnitType ?? string.Empty;
    }
}
