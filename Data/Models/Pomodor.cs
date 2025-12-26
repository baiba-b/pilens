using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pilens.Data.Models
{
    public class Pomodoro
    {
        [Key]
        [ForeignKey(nameof(User))]

        public int DurationInMinutes { get; set; }

        public int Minutes { get; set; } = 25;
        public int PauseMinutes { get; set; } = 5;
        public int LongPauseMinutes { get; set; } = 20;
        public int SessionAmount { get; set; } = 4;
        public int SessionLongPause { get; set; } = 4;
        public int AdjustedMin { get; set; } = 5;
        public string UserID { get; set; } = string.Empty;

        public virtual ApplicationUser User { get; set; } = null!;

    }
}
