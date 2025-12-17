using System.ComponentModel.DataAnnotations;
namespace Pilens.Data.Models

{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Grupas nosaukums nevar būt garāks par 100 simboliem.")]
        public string Name { get; set; } = string.Empty;
        public ICollection<ToDoTask> ToDoTasks { get; set; } = new List<ToDoTask>();

        public Group ()
        {
        }

      
        public Group (string name, IEnumerable<ToDoTask>? toDoTasks)
        {
            Name = name ?? string.Empty;
            ToDoTasks = toDoTasks != null ? new List<ToDoTask>(toDoTasks) : new List<ToDoTask>();
        }

       
    
    }
}