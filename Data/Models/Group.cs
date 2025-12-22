using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;
namespace Pilens.Data.Models

{
    public class Group : IEquatable<Group>
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Grupas nosaukums nevar būt garāks par 100 simboliem.")]
        public string Name { get; set; } = "Vispārīgā";
        public ICollection<ToDoTask> ToDoTasks { get; set; } = new List<ToDoTask>();

        public Group ()
        {
        }

      
        public Group (string name, IEnumerable<ToDoTask>? toDoTasks)
        {
            Name = name ?? string.Empty;
            ToDoTasks = toDoTasks != null ? new List<ToDoTask>(toDoTasks) : new List<ToDoTask>();
        }

        public override bool Equals(object? obj) => obj is Group group && Equals(group);

        public bool Equals(Group? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => Name;
    }
}