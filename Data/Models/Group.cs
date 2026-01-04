using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Pilens.Data.Models

{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public required string Name { get; set; } = "Vispārīgā";
        [InverseProperty(nameof(ToDoTaskGroup.Group))]
        public ICollection<ToDoTaskGroup> ToDoTaskGroups{ get; set; } = new List<ToDoTaskGroup>();
        public override string ToString() => Name;

    }
}