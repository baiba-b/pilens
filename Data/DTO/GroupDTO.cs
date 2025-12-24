using Pilens.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Pilens.Data.DTO;

public class GroupDTO : IEquatable<GroupDTO>
{
    public int Id { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "Grupas nosaukums nevar būt garāks par 100 simboliem.")]
    public string Name { get; set; } = "Vispārīgā";
    public ICollection<ToDoTask> ToDoTasks { get; set; } = new List<ToDoTask>();
    public GroupDTO()
    {
    }
    public GroupDTO(string name, IEnumerable<ToDoTask>? toDoTasks)
    {
        Name = name ?? string.Empty;
        ToDoTasks = toDoTasks != null ? new List<ToDoTask>(toDoTasks) : new List<ToDoTask>();
    }
    public override bool Equals(object? obj) => obj is GroupDTO group && Equals(group);

    /// <summary>
    /// Konstruktors, kas pārveido entitāti uz db objektu
    /// </summary>
    /// <param name="group">Grupa</param>
    /// <returns></returns>
    public GroupDTO(Group group)
    {
        this.Id = group.Id;
        this.Name = group.Name;
    }

    public bool Equals(GroupDTO? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => Name;
}
