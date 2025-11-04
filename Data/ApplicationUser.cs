using Microsoft.AspNetCore.Identity;
using Pilens.Data.Models;

namespace Pilens.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public Pomodoro? Pomodoros { get; set; }
    }

}
