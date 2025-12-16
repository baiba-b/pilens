namespace Pilens.Data.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<ToDoTask> ToDoTasks { get; set; } = new List<ToDoTask>();
    }
}