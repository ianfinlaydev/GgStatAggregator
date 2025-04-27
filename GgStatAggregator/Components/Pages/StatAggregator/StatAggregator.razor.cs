using GgStatAggregator.Models;
using GgStatAggregator.Result;
using GgStatAggregator.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using System.Threading.Tasks;

namespace GgStatAggregator.Components.Pages.StatAggregator
{
    public partial class StatAggregator(
        IService<Player> playerService, 
        IService<StatSet> statSetService, 
        IService<Table> tableService, 
        IDialogService dialogService,
        IJSRuntime js,
        ILogger<StatAggregator> logger,
        StatAggregatorForm form) : ComponentBase
    {
        private readonly IService<Player> PlayerService = playerService;
        private readonly IService<StatSet> StatSetService = statSetService;
        private readonly IService<Table> TableService = tableService;
        private readonly IDialogService DialogService = dialogService;
        private readonly IJSRuntime JS = js;
        private readonly ILogger<StatAggregator> Logger = logger;
        private readonly StatAggregatorForm Form= form;

        private MudAutocomplete<string> PlayerAutocomplete;
        private MudNumericField<int> HandNumericField;
        private MudNumericField<int> TableNumericField;
        private EditForm editForm;

        private bool _isFormDisabled = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && PlayerAutocomplete is not null)
            {
                await TableNumericField.FocusAsync();
            }
        }

        private async Task<IEnumerable<string>> SearchNamesAsync(string value, CancellationToken cancellationToken)
        {
            var result = await PlayerService.GetAllAsync(p => EF.Functions.Like(p.Name, $"%{value}%"));
            return result.Value.Select(p => p.Name);
        }

        private async Task BeforeSubmit()
        {
            // Try to find existing table
            var tableResult = await TableService.GetFirstOrDefaultAsync(
                t => t.Stake == Form.SelectedStake && t.TableNumber == Form.SelectedTableNumber);

            // If no table found
            if (tableResult.IsFailure)
            {
                tableResult = await TableService.StageAsync(new Table
                {
                    Stake = Form.SelectedStake,
                    TableNumber = Form.SelectedTableNumber
                });
            }

            // If table was staged successfully
            if (tableResult.IsSuccess)
            {
                Form.SelectedTable = tableResult.Value;
            }


            // Try to find existing player
            var playerResult = await PlayerService.GetFirstOrDefaultAsync(p => p.Name == Form.SelectedName);

            // If no player found
            if (playerResult.IsFailure)
            {
                // Prompt user to continue with new player
                var dialogResult = await DialogService.ShowMessageBox(
                    "Create Player",
                    $"Player: [{Form.SelectedName}] does not exist. Create new player?",
                    yesText: "Yes",
                    cancelText: "Cancel",
                    options: new DialogOptions { CloseOnEscapeKey = true });

                // User did not want to continue with new player
                if (dialogResult != true)
                    return;

                // Stage a new player
                playerResult = await PlayerService.StageAsync(new Player { Name = Form.SelectedName });
            }

            // If player was staged successfully
            if (playerResult.IsSuccess)
            {
                Form.SelectedPlayer = playerResult.Value;
            }

            if (!editForm.EditContext.Validate())
            {
                await StatSetService.ClearStagedChanges();
            }

            await HandleValidSubmit();
        }

        private async Task HandleValidSubmit()
        {
            //Stage new stat set
            var statSetResult = await StatSetService.StageAsync(new StatSet
            {
                PlayerId = Form.SelectedPlayer.Id,
                TableId = Form.SelectedTable.Id,
                Hands = Form.Hands,
                Vpip = Form.Vpip,
                Pfr = Form.Pfr,
                Steal = Form.Steal,
                ThreeBet = Form.ThreeBet
            });

            // If staging failed, show error message
            if (statSetResult.IsFailure)
            {
                await DialogService.ShowMessageBox("Error",
                    $"Failed to stage stat set: {statSetResult.Message}",
                    yesText: "OK",
                    options: new DialogOptions { CloseOnEscapeKey = true });
                return;
            }

            await StatSetService.CommitAsync();

            // Build player note
            Form.PlayerNote = Form.SelectedPlayer.ToString();

            // Disable the form after submission
            _isFormDisabled = true;

            // Refresh the UI
            StateHasChanged();

            // Show success message
            await DialogService.ShowMessageBox("Success", 
                $"Stats for {Form.SelectedPlayer.Name} added successfully.", 
                yesText: "OK",
                options: new DialogOptions { CloseOnEscapeKey = true });
        }

        private async Task HandleKeyDownAsync(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                //Allow default behaviour
            }
            else if (args.Key == "Tab")
            {
                Form.SelectedName = PlayerAutocomplete.Text;
                await PlayerAutocomplete.CloseMenuAsync();

                if (args.ShiftKey)
                    await TableNumericField.FocusAsync();
                else
                    await HandNumericField.FocusAsync();
            }
            else if (args.Key == "Escape")
            {
                await PlayerAutocomplete.CloseMenuAsync();
                // Give the dropdown time to close for Blur to work
                await Task.Delay(100); 
                await PlayerAutocomplete.BlurAsync();
            }
        }

        private async Task CopyPlayerNote() 
            => await JS.InvokeVoidAsync("navigator.clipboard.writeText", Form.PlayerNote);

        private async Task EnableForm() 
        {
            await PlayerAutocomplete.ClearAsync();
            Form.Clear();
            _isFormDisabled = false;
            await TableNumericField.FocusAsync();
        }
    }
}
