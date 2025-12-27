using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Pilens.Data;
using Pilens.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pilens.Components.Pages.todo;

public partial class ToDoUpdate
{
    [Inject]
    private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public int TaskId { get; set; }

    private ToDoTask? task;

    private string? ErrorMessage { get; set; }

    // TODO: change to DTO
    private IEnumerable<Group> selectedGroups = new HashSet<Group>();
    private List<Group> groups = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();


            groups = await db.Groups
                .AsNoTracking()
                .ToListAsync();

            var task2 = await db.ToDoTasks
                .Include(x => x.ToDoTaskGroups)
                .ThenInclude(x => x.Group)
                .FirstOrDefaultAsync(x => x.Id == TaskId);

            if (task2 == null)
            {
                ErrorMessage = "Uzdevums netika atrasts";
                task = null;
                return;
            }

            selectedGroups = task2.ToDoTaskGroups.Select(tg => tg.Group).ToList();
            task = task2;
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Kļūda ielādējot datus: {ex.Message}";
            task = null;
        }
    }

    private async Task UpdateTask()
    {
        if (task is null)
        {
            ErrorMessage = "Uzdevums netika atrasts";
            return;
        }

        try
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();

            var dbTask = await db.ToDoTasks
                .Include(x => x.ToDoTaskGroups)
                .FirstOrDefaultAsync(x => x.Id == task.Id);

            if (dbTask == null)
            {
                ErrorMessage = "Uzdevums netika atrasts";
                return;
            }

            dbTask.Title = task.Title;
            dbTask.Description = task.Description;
            dbTask.IsCompleted = task.IsCompleted;
            dbTask.Effort = task.Effort;
            dbTask.Deadline = task.Deadline;
            dbTask.EffortDuration = task.EffortDuration;
            dbTask.Identifier = task.Identifier;
            dbTask.SessionsRequired = task.SessionsRequired;
            dbTask.ProgressTargetUnits = task.ProgressTargetUnits;
            dbTask.ProgressCurrentUnits = task.ProgressCurrentUnits;
            dbTask.ProgressUnitType = task.ProgressUnitType;

            var selectedIds = selectedGroups.Select(g => g.Id).ToHashSet();
            var existingIds = dbTask.ToDoTaskGroups.Select(tg => tg.GroupId).ToList();

            var addedIds = selectedIds.Except(existingIds).ToList();
            var removedIds = existingIds.Except(selectedIds).ToList();

            foreach (var groupId in addedIds)
            {
                db.ToDoTaskGroups.Add(new ToDoTaskGroup
                {
                    ToDoTaskId = dbTask.Id,
                    GroupId = groupId
                });
            }

            foreach (var groupId in removedIds)
            {
                var toDoTaskGroup = await db.ToDoTaskGroups
                    .FirstOrDefaultAsync(tg => tg.ToDoTaskId == dbTask.Id && tg.GroupId == groupId);

                if (toDoTaskGroup != null)
                {
                    db.ToDoTaskGroups.Remove(toDoTaskGroup);
                }
            }

            await db.SaveChangesAsync();
            Navigation.NavigateTo("/");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Kļūda! Nevarēja saglabāt izmaiņas: {ex.Message}";
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/ToDo");
    }
}
