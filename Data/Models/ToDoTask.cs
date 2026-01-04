using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Pilens.Data.Models;

public partial class ToDoTask
{
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(200)]
    public string Title { get; set; } 
    public string? Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    [Range(int.MinValue, 3)]
    public int Effort { get; set; } = 1;
    [Required]
    public DateTime? Deadline { get; set; } = DateTime.Now;
    [Required]
    public TimeSpan EffortDuration { get; set; } = TimeSpan.FromMinutes(30);
    [InverseProperty(nameof(ToDoTaskGroup.ToDoTask))]  
    public ICollection<ToDoTaskGroup> ToDoTaskGroups { get; set; } = new List<ToDoTaskGroup>();

    [Required]
    [ForeignKey(nameof(UserID))]
    public string UserID { get; set; } = string.Empty;
    public virtual ApplicationUser? User { get; set; }

    public string Identifier { get; set; } = "Saraksts";

    public int SessionsRequired { get; set; } = 0;

    public int ProgressTargetUnits { get; set; } = 0;
    public int ProgressCurrentUnits { get; set; } = 0;
    public string ProgressUnitType { get; set; } = "lpp";

   
}
