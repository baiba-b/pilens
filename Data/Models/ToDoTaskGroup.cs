using System.ComponentModel.DataAnnotations;

namespace Pilens.Data.Models
{
    public class ToDoTaskGroup
    {
        [Key]
        public int Id { get; set; }
        public int ToDoTaskId { get; set; }
        public int GroupId { get; set; }
    }
}
