using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
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
    private string? ToDoTaskReqError { get; set; } = "Šis atribūts ir obligāts!";
    private MudForm? todoForm;
    private bool _formIsValid = true;

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
                ErrorMessage = "Uzdevums netika atrasts!";
                task = null;
                return;
            }

            selectedGroups = task2.ToDoTaskGroups.Select(tg => tg.Group).ToList();
            task = task2;
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Neizdevās ielādēt grupas!";
            task = null;
        }
    }

    private async Task UpdateTask()
    {
        if (task is null)
        {
            ErrorMessage = "Uzdevums netika atrasts!";
            return;
        }

        if (todoForm is not null)
        {
            await todoForm.Validate();
            if (!todoForm.IsValid)
            {
                SnackbarService.Add("Lūdzu, ievadiet korektus datus pirms saglabāšanas.", Severity.Error);
                return;
            }
        }

        try
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();

            var dbTask = await db.ToDoTasks
                .Include(x => x.ToDoTaskGroups)
                .FirstOrDefaultAsync(x => x.Id == task.Id);

            if (dbTask == null)
            {
                ErrorMessage = "Uzdevums netika atrasts!";
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
            ErrorMessage = "Kļūda! Nevarēja saglabāt izmaiņas!";
            SnackbarService.Add(ErrorMessage, Severity.Error);
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/ToDo");
    }

    private string TitleValidation(string? value)
    {
        var len = value.Trim().Length;
        if (len < 1 || len > 200)
            return "Uzdevuma nosaukumam jābūt 1–200 simbolu garam!";
        return string.Empty;
    }

    private string DescriptionValidation(string? value)
    {
        if (value is null)
            return string.Empty;
        var trimmed = value.Trim();
        if (trimmed.Length > 500)
            return "Apraksts nevar būt garāks par 500 simboliem!";
        return string.Empty;
    }

    private string DeadlineValidation(DateTime value)
    {
        var today = DateTime.Today;
        if (value.Date < today)
            return "Datums nevar būt pagātnē.";
        return string.Empty;
    }

    private string EffortValidation(int value)
    {
        if (value < 1 || value > 3)
            return "Grūtības pakāpei jābūt no 1 līdz 3.";
        return string.Empty;
    }

    private string EffortDurationValidation(TimeSpan value)
    {
        if (value >= TimeSpan.FromHours(24))
            return "Laikam jābūt 24-stundu formātā (HH:MM).";
        return string.Empty;
    }

    private string ProgressTargetValidation(int value)
    {
        if (value < 0)
            return "Progresa mērķa vienībām jābūt pozitīvam skaitlim.";
        return string.Empty;
    }

    private string ProgressCurrentValidation(int value)
    {
        if (value < 0)
            return "Progresa esošajām vienībām jābūt pozitīvam skaitlim.";
        if (task is not null && task.ProgressTargetUnits >= 0 && value > task.ProgressTargetUnits)
            return "Esošās vienības nevar pārsniegt mērķa vienības.";
        return string.Empty;
    }

    private string ProgressUnitTypeValidation(string? value)
    {
        if (value is null)
            return string.Empty;
        if (value.Trim().Length > 100)
            return "Vienības tips nevar būt garāks par 100 simboliem.";
        return string.Empty;
    }
}
