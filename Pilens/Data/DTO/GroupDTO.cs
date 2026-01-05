using Pilens.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Pilens.Data.DTO;

public class GroupDTO : IEquatable<GroupDTO>
{
    public int Id { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "Grupas nosaukums nevar būt garāks par 100 simboliem.")]
    public string Name { get; set; } = "Vispārīgā"; 
    public ICollection<ToDoTaskGroup> ToDoTaskGroups { get; set; } = new List<ToDoTaskGroup>();
    public string UserID { get; set; } = string.Empty;
    public virtual ApplicationUser? User { get; set; }
    public GroupDTO()
    {
    }

    public GroupDTO(string name, IEnumerable<ToDoTaskGroup>? toDoTasksGroup)
    {
        Name = name ?? string.Empty;
        ToDoTaskGroups = toDoTasksGroup != null ? toDoTasksGroup.ToList() : new List<ToDoTaskGroup>();
    }
    /// <summary>
    /// Konstruktors, kas pārveido entitāti uz db objektu
    /// </summary>
    /// <param name="group">Grupa</param>
    /// <returns></returns>
    public GroupDTO(Group group)
    {
        Id = group.Id;
        Name = group.Name;
        UserID = group.UserID ?? string.Empty;
    }

    public override bool Equals(object? obj) => obj is GroupDTO group && Equals(group);

    public bool Equals(GroupDTO? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => Name;
}
