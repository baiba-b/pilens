using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pilens.Data.Models;

namespace Pilens.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Pomodoro> Pomodoros { get; set; }
        public DbSet<ToDoTask> ToDoTasks { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<ToDoTaskGroup> ToDoTaskGroups { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

       
            builder.Entity<ToDoTask>()
                .HasMany(e => e.Groups)
                .WithMany(e => e.ToDoTasks)
                .UsingEntity<ToDoTaskGroup>();

           

        }
    }
}
