namespace Pilens.Data.Models
{
    public partial class ToDoTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public int Effort { get; set; } = 0;
        public DateTime Deadline { get; set; } = DateTime.Now;
        public TimeSpan EffortDuration { get; set; } = TimeSpan.Zero;

        public ICollection<Group> Groups { get; set; } = new List<Group>();

        public string Identifier { get; set; } = "Saraksts";

        public int SessionsRequired { get; set; } = 0;

        public int ProgressTargetUnits { get; set; } = 0;
        public int ProgressCurrentUnits { get; set; } = 0;
        public string ProgressUnitType { get; set; } = "lpp";
    }
}
