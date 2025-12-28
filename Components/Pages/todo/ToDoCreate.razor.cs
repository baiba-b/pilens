using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.DTO;
using Pilens.Data.Models;

namespace Pilens.Components.Pages.todo;

public partial class ToDoCreate
{
    [Inject]
    private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

    private ToDoTaskDTO todoTaskDto = new();

    private string userId { get; set; }
    private string? ErrorMessage { get; set; }
    private string? ToDoTaskReqError { get; set; } = "Šis atribūts ir obligāts!";
    private MudForm? todoForm;
    private bool _formIsValid = true;

    protected override void OnInitialized()
    {
        todoTaskDto = new ToDoTaskDTO();
    }

    List<GroupDTO> groups = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();
            groups = await db.Groups
                .Select(g => new GroupDTO(g))
                .ToListAsync();
            var uid = await getUserId();

            todoTaskDto.UserID = uid;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Neizdevās ielādēt grupas: " + ex.Message;
            SnackbarService.Add(ErrorMessage, Severity.Error);
        }
    }

    private IEnumerable<GroupDTO> selectedGroups = new HashSet<GroupDTO>();

    private async Task CreateTask()
    {
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

            var task = new ToDoTask
            {
                Title = todoTaskDto.Title,
                Description = todoTaskDto.Description,
                IsCompleted = todoTaskDto.IsCompleted,
                Effort = todoTaskDto.Effort,
                Deadline = todoTaskDto.Deadline,
                EffortDuration = todoTaskDto.EffortDuration,
                Identifier = todoTaskDto.Identifier,
                SessionsRequired = todoTaskDto.SessionsRequired,
                ProgressTargetUnits = todoTaskDto.ProgressTargetUnits,
                ProgressCurrentUnits = todoTaskDto.ProgressCurrentUnits,
                ProgressUnitType = todoTaskDto.ProgressUnitType,
                UserID = todoTaskDto.UserID
            };

            db.ToDoTasks.Add(task);
            await db.SaveChangesAsync();

            foreach (var groupDto in selectedGroups)
            {
                var toDoTaskGroup = new ToDoTaskGroup
                {
                    ToDoTaskId = task.Id,
                    GroupId = groupDto.Id
                };
                db.ToDoTaskGroups.Add(toDoTaskGroup);
            }

            await db.SaveChangesAsync();
            Navigation.NavigateTo("/");
            todoTaskDto = new ToDoTaskDTO();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Neizdevās izveidot uzdevumu: " + ex.Message;
            SnackbarService.Add(ErrorMessage, Severity.Error);
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/ToDo");
    }

    async Task<string> getUserId()
    {
        var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var UserId = user.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;
        if (string.IsNullOrWhiteSpace(UserId))
        {
            SnackbarService.Add("Neizdevās identificēt lietotāju.", Severity.Error);
            return UserId!;
        }
        return UserId!;
    }


    private string TitleValidation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Lūdzu, ievadi uzdevuma nosaukumu.";
        var len = value.Trim().Length;
        if (len < 1 || len > 200)
            return "Uzdevuma nosaukumam jābūt 1–200 simbolu garam.";
        return string.Empty;
    }

    private string DescriptionValidation(string? value)
    {
        if (value is null)
            return string.Empty;
        var trimmed = value.Trim();
        if (trimmed.Length > 500)
            return "Apraksts nevar būt garāks par 500 simboliem.";
        return string.Empty;
    }

    private string DeadlineValidation(DateTime value)
    {
        var today = DateTime.Today;
        if (value.Date < today)
            return "Datums nevar būt pagātnē.";
        return string.Empty;
    }

    // Effort: 1–3
    private string EffortValidation(int value)
    {
        if (value < 1 || value > 3)
            return "Grūtības līmenim jābūt no 1 līdz 3.";
        return string.Empty;
    }

    // EffortDuration: HH:MM, must be positive and within 24h mask
    private string EffortDurationValidation(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
            return "Pieprasītais laiks ir obligāts un tam jābūt pozitīvam (HH:MM).";
        if (value >= TimeSpan.FromHours(24))
            return "Laiks jābūt 24-stundu formātā (HH:MM).";
        return string.Empty;
    }

    // Progress target: optional, positive (>= 0)
    private string ProgressTargetValidation(int value)
    {
        if (value < 0)
            return "Progresa mērķa vienībām jābūt pozitīvam skaitlim.";
        return string.Empty;
    }

    // Progress current: optional, positive (>= 0), not exceeding target
    private string ProgressCurrentValidation(int value)
    {
        if (value < 0)
            return "Progresa esošajām vienībām jābūt pozitīvam skaitlim.";
        if (todoTaskDto.ProgressTargetUnits >= 0 && value > todoTaskDto.ProgressTargetUnits)
            return "Esošās vienības nevar pārsniegt mērķa vienības.";
        return string.Empty;
    }

    // Progress unit type: optional
    private string ProgressUnitTypeValidation(string? value)
    {
        // Optional; reject whitespace-only if provided
        if (value is null)
            return string.Empty;
        if (value.Trim().Length == 0)
            return "Vienības tips nevar sastāvēt tikai no atstarpēm.";
        return string.Empty;
    }
}
