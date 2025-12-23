using System.ComponentModel.DataAnnotations;

namespace Pilens.Data.Models
{
    public class ToDoTaskGroup
    {
        [Key]
        public int Id { get; set; }
        public ToDoTask ToDoTask { get; set; } = null!;
        public int ToDoTaskId { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;
    }
}
