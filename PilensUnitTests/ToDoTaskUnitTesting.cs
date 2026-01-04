using Microsoft.EntityFrameworkCore;
using Pilens.Data;
using Pilens.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace PilensUnitTests;


public sealed class ToDoTaskUnitTesting
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = "user@test.com",
            NormalizedUserName = "USER@TEST.COM",
            Email = "user@test.com",
            NormalizedEmail = "USER@TEST.COM",
            EmailConfirmed = true
        };
        context.Users.Add(user);
        context.SaveChanges();
        return context;
    }

    private static ToDoTask CreateValidTask(string userId) =>
        new()
        {
            Title = "Test Task",
            Description = "Desc",
            Effort = 2,
            Deadline = DateTime.UtcNow.AddDays(1),
            EffortDuration = TimeSpan.FromMinutes(50),
            Identifier = "Saraksts",
            SessionsRequired = 0,
            ProgressTargetUnits = 10,
            ProgressCurrentUnits = 2,
            ProgressUnitType = "lpp",
            UserID = userId
        };

    [Fact]
    public void VT_Uzd_01_CreateWithoutRequiredData_FailsValidation()
    {
        var task = new ToDoTask
        {
            Title = null!,
            Deadline = null, 
            UserID = "user-1"
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(task, new ValidationContext(task), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ToDoTask.Title)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ToDoTask.Deadline)));
    }

    [Fact]
    public async Task VT_Uzd_02_CreateWithRequiredData_SucceedsAndIsListable()
    {
        using var db = CreateContext(nameof(VT_Uzd_02_CreateWithRequiredData_SucceedsAndIsListable));

        var task = CreateValidTask("user-1");
        db.ToDoTasks.Add(task);
        await db.SaveChangesAsync();

        var stored = await db.ToDoTasks.Where(t => t.UserID == "user-1").ToListAsync();
        Assert.Single(stored);
        Assert.Equal(task.Title, stored[0].Title);
    }

    [Fact]
    public async Task VT_Uzd_03_DeleteTask_RemovesFromList()
    {
        using var db = CreateContext(nameof(VT_Uzd_03_DeleteTask_RemovesFromList));

        var task = CreateValidTask("user-1");
        db.ToDoTasks.Add(task);
        await db.SaveChangesAsync();

        db.ToDoTasks.Remove(task);
        await db.SaveChangesAsync();

        Assert.Empty(await db.ToDoTasks.Where(t => t.UserID == "user-1").ToListAsync());
    }

    [Fact]
    public void VT_Uzd_04_UpdateMissingRequired_FailsValidation()
    {
        var task = CreateValidTask("user-1");
        task.Title = "ValidTitle"; 

        task.Title = string.Empty; 

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(task, new ValidationContext(task), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ToDoTask.Title)));
    }

    [Fact]
    public async Task VT_Uzd_05_UpdateWithValidData_PersistsChanges()
    {
        using var db = CreateContext(nameof(VT_Uzd_05_UpdateWithValidData_PersistsChanges));

        var task = CreateValidTask("user-1");
        db.ToDoTasks.Add(task);
        await db.SaveChangesAsync();

        task.Title = "Updated title";
        task.ProgressCurrentUnits = 5;
        db.ToDoTasks.Update(task);
        await db.SaveChangesAsync();

        var updated = await db.ToDoTasks.FindAsync(task.Id);
        Assert.Equal("Updated title", updated!.Title);
        Assert.Equal(5, updated.ProgressCurrentUnits);
    }

    [Fact]
    public async Task VT_Uzd_06_IncreaseProgress_IncrementsAndPersists()
    {
        using var db = CreateContext(nameof(VT_Uzd_06_IncreaseProgress_IncrementsAndPersists));

        var task = CreateValidTask("user-1");
        task.ProgressTargetUnits = 10;
        task.ProgressCurrentUnits = 2;
        db.ToDoTasks.Add(task);
        await db.SaveChangesAsync();

        task.ProgressCurrentUnits += 1;
        db.Update(task);
        await db.SaveChangesAsync();

        var stored = await db.ToDoTasks.FindAsync(task.Id);
        Assert.Equal(3, stored!.ProgressCurrentUnits);
    }

    [Fact]
    public async Task VT_Uzd_07_DecreaseProgress_DecrementsAndPersists()
    {
        using var db = CreateContext(nameof(VT_Uzd_07_DecreaseProgress_DecrementsAndPersists));

        var task = CreateValidTask("user-1");
        task.ProgressTargetUnits = 10;
        task.ProgressCurrentUnits = 5;
        db.ToDoTasks.Add(task);
        await db.SaveChangesAsync();

        task.ProgressCurrentUnits -= 1;
        db.Update(task);
        await db.SaveChangesAsync();

        var stored = await db.ToDoTasks.FindAsync(task.Id);
        Assert.Equal(4, stored!.ProgressCurrentUnits);
    }

    [Fact]
    public async Task VT_Uzd_08_MoveToSessions_ComputesSessionsAndKeepsIdentifier()
    {
        using var db = CreateContext(nameof(VT_Uzd_08_MoveToSessions_ComputesSessionsAndKeepsIdentifier));

        var task = CreateValidTask("user-1");
        task.EffortDuration = TimeSpan.FromMinutes(50); 
        db.ToDoTasks.Add(task);
        await db.SaveChangesAsync();

        task.Identifier = "Sesijas";
        var pomodoroLength = 25;
        task.SessionsRequired = (int)Math.Ceiling(task.EffortDuration.TotalMinutes / pomodoroLength);
        db.Update(task);
        await db.SaveChangesAsync();

        var stored = await db.ToDoTasks.FindAsync(task.Id);
        Assert.Equal("Sesijas", stored!.Identifier);
        Assert.Equal(2, stored.SessionsRequired);
    }

    [Fact]
    public async Task VT_Uzd_09_ToggleCompletion_MarksCompleted()
    {
        using var db = CreateContext(nameof(VT_Uzd_09_ToggleCompletion_MarksCompleted));

        var task = CreateValidTask("user-1");
        task.IsCompleted = false;
        db.ToDoTasks.Add(task);
        await db.SaveChangesAsync();

        task.IsCompleted = true;
        db.Update(task);
        await db.SaveChangesAsync();

        var stored = await db.ToDoTasks.FindAsync(task.Id);
        Assert.True(stored!.IsCompleted);
    }
}
