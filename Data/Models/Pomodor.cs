using System.ComponentModel.DataAnnotations;

namespace Pilens.Data.Models
{
    public class Pomodoro
    {
        [Key]
        public int Id { get; set; }
        public int DurationInMinutes { get; set; }

        public Pomodoro() { }

        public Pomodoro(int id, int durationInMinutes)
        {
            Id = id;
            DurationInMinutes = durationInMinutes;
        }
    }
}
