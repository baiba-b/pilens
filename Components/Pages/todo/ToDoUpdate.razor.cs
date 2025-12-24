using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Pilens.Data;
using Pilens.Data.Models;

namespace Pilens.Components.Pages.todo
{
    public partial class ToDoUpdate
    {
        [Inject]
        private ApplicationDbContext Db { get; set; } = default!;
        [Parameter]
        public int TaskId { get; set; }

        private ToDoTask todoTask = new();

        private string? ErrorMessage { get; set; }

        private IEnumerable<Pilens.Data.Models.Group> selectedGroups = new HashSet<Pilens.Data.Models.Group>();
        List<Pilens.Data.Models.Group> groups = new();

        protected override async Task OnParametersSetAsync()
        {
            var selectedGroups = Db.ToDoTaskGroups
                .Include(tg => tg.Group)
                .ThenInclude(g => g.ToDoTasks)
                .ToList();

            groups = await Db.Groups.ToListAsync();

            var task = await Db.ToDoTasks.FindAsync(TaskId);
            if (task == null)
            {
                ErrorMessage = "Task not found.";
                todoTask = new ToDoTask();
                return;
            }

            ErrorMessage = null;
            todoTask = task;

        }

        private async Task UpdateTask()
        {
            try
            {
                Db.ToDoTasks.Update(todoTask);
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
}
