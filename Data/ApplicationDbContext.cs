using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pilens.Data.Models;
using System.Reflection;

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
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


            builder.Entity<ToDoTask>()
                .HasOne(t => t.User)
                .WithMany(u => u.ToDoTasks)
                .HasForeignKey(t => t.UserID)
                .IsRequired();
        }
    }
}
