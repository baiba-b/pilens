using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Pilens.Data;
using Pilens.Data.Models;

namespace Pilens.Components.Pages.todo;

public partial class ToDoUpdate
{
    [Inject]
    private ApplicationDbContext Db { get; set; } = default!;
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;
    [Parameter]
    public int TaskId { get; set; }

    private ToDoTask task = null!;

    private string? ErrorMessage { get; set; }

    // TODO: change to DTO
    private IEnumerable<Pilens.Data.Models.Group> selectedGroups = new HashSet<Pilens.Data.Models.Group>();
    List<Pilens.Data.Models.Group> groups = new();

    protected override void OnInitialized()
    {

        groups = Db.Groups.ToList();

        var task2 = Db.ToDoTasks
            .Include(x => x.ToDoTaskGroups)
            .ThenInclude(x => x.Group)
            .FirstOrDefault(x => x.Id == TaskId);
        if (task2 == null)
        {
            ErrorMessage = "Uzdevums netika atrasts";
            // TODO: fix navigate to create
            //todoTask = new ToDoTask();
            return;
        }
        selectedGroups = task2.ToDoTaskGroups.Select(tg => tg.Group).ToList();
        ErrorMessage = null;
        task = task2;
    }

    private async Task UpdateTask()
    {
        try
        {
            var addedGroups = selectedGroups.Except(task.ToDoTaskGroups.Select(tg => tg.Group)).ToList();
            var removedGroups = task.ToDoTaskGroups.Select(tg => tg.Group).Except(selectedGroups).ToList();
            foreach (var group in addedGroups)
            {
                var toDoTaskGroup = new ToDoTaskGroup
                {
                    ToDoTaskId = task.Id,
                    GroupId = group.Id
                };
                Db.ToDoTaskGroups.Add(toDoTaskGroup);
            }
            foreach (var group in removedGroups)
            {
                var toDoTaskGroup = await Db.ToDoTaskGroups
                    .FirstOrDefaultAsync(tg => tg.ToDoTaskId == task.Id && tg.GroupId == group.Id);
                if (toDoTaskGroup != null)
                {
                    Db.ToDoTaskGroups.Remove(toDoTaskGroup);
                }
            }
            Db.ToDoTasks.Update(task);
            await Db.SaveChangesAsync();
            Navigation.NavigateTo("/");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Kļūda! Nevarēja sagla'bāt izmaiņas: {ex.Message}";
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/ToDo");
    }
}
