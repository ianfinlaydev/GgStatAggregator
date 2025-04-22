using GgStatAggregator.Data;
using GgStatAggregator.Models;
using GgStatAggregator.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace GgStatAggregator.Components.Dialogs
{
    public partial class AddPlayerDialog(GgStatAggregatorDbContext context, IDialogService dialogService) : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        private readonly IDialogService DialogService = dialogService;
        private readonly GgStatAggregatorDbContext DbContext = context;

        private AddPlayerForm Model { get; set; } = new();

        private async Task OnValidSubmit(EditContext editContext)
        {
            bool nameExists = await DbContext.Players
                .AnyAsync(p => p.Name.ToLower() == Model.Name.ToLower());

            if (nameExists)
            {
                await DialogService.ShowMessageBox("Error", "Player with this name already exists.", yesText: "OK");
                return;
            }

            MudDialog.Close(DialogResult.Ok(Model.Name));
        }

        private void Cancel() => MudDialog.Cancel();
    }

    public class AddPlayerForm
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
    }
}
