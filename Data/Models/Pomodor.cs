using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pilens.Data.Models
{
    public class Pomodoro
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string UserID { get; set; } = string.Empty;

        public int DurationInMinutes { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public Pomodoro() { }

        public Pomodoro(ApplicationUser user, int durationInMinutes)
        {
            this.User = user;
            this.UserID = user.Id;
            this.DurationInMinutes = durationInMinutes;
        }
    }
}
