using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pilens.Data.Models;

public class ToDoTaskGroup
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(ToDoTaskId))]
    public ToDoTask ToDoTask { get; set; } = null!;
    public required int ToDoTaskId { get; set; }
    public required int GroupId { get; set; }
    [ForeignKey(nameof(GroupId))]
    public Group Group { get; set; } = null!;
}
