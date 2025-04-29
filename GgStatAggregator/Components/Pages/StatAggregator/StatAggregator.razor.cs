using GgStatAggregator.Models;
using GgStatAggregator.Models.Extensions;
using GgStatAggregator.Result;
using GgStatAggregator.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;

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

        private MudAutocomplete<string?>? PlayerAutocomplete;
        private MudNumericField<int?>? TableNumericField;
        private EditForm? EditForm;

        private bool _isFormDisabled = false;

        private async Task BeforeSubmit()
        {
            var tableResult = await PrepareTable();
            if (tableResult.IsFailure)
                return;

            var playerResult = await PreparePlayer();
            if (playerResult.IsFailure)
                return;

            var statSetResult = await PrepareStatSet();
            if (statSetResult.IsFailure)
                return;

            if (EditForm!.EditContext!.Validate() == true)
            {
                await HandleValidSubmit();
            }
            else
            {
                await StatSetService.ClearStagedChanges();
            }
        }

        private async Task<Result<Table?>> PrepareTable()
        {
            // Try to find existing table
            Result<Table?> tableResult = await TableService.GetFirstOrDefaultAsync(
                t => t!.Stake == Form.SelectedStake && t.TableNumber == Form.SelectedTableNumber);

            // If no table found
            if (tableResult.IsFailure)
            {
                // Stage new table
                tableResult = await TableService.StageAsync(new Table
                {
                    Stake = Form.SelectedStake,
                    // Assign -1 as default invalid value; form validation will catch and block invalid submissions
                    TableNumber = Form.SelectedTableNumber ?? -1 
                }, StageAction.Add);
            }

            // If table was staged unsuccessfully
            if (tableResult.IsFailure)
            {
                await TableService.ClearStagedChanges();
                return tableResult;
            }

            //Table was staged successfully
            Form.SelectedTable = tableResult.Value!;
            return tableResult;
        }

        private async Task<Result<Player?>> PreparePlayer()
        {
            // Try to find existing player
            var playerResult = await PlayerService.GetFirstOrDefaultAsync(p => p!.Name == Form.SelectedName);

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

                // Stage a new player if dialog result is true
                playerResult = dialogResult != true
                    ? playerResult
                    // Assign string.Empty as default invalid value; form validation will catch and block invalid submissions
                    : await PlayerService.StageAsync(new Player { Name = Form.SelectedName ?? string.Empty }, StageAction.Add);
            }

            // If player was staged unsuccessfully
            if (playerResult.IsFailure)
            {
                await PlayerService.ClearStagedChanges();
                return playerResult;
            }

            // Player was staged successfully
            Form.SelectedPlayer = playerResult.Value!;
            return playerResult;
        }

        private async Task<Result<StatSet?>> PrepareStatSet()
        {
            // Try to find existing stat set
            var statSetResult = await StatSetService.GetFirstOrDefaultAsync(
                s => s!.PlayerId == Form.SelectedPlayer!.Id && s.TableId == Form.SelectedTable!.Id,
                orderBy: q => q.OrderByDescending(s => s!.CreatedAt));

            // If existing stat set found with for player and table
            // prompt user to update or add new
            if (statSetResult.IsSuccess)
            {
                // Assign -1 as default invalid values; form validation will catch and block invalid submissions
                var existingStatSet = statSetResult.Value!;
                if (existingStatSet.IsPossibleDuplicate(Form.Hands ?? -1))
                {
                    var dialogResult = await DialogService.ShowMessageBox(
                        "Possible Duplicate",
                        $"A stat set for [{Form.SelectedName}] at [{Form.SelectedTable}] was already " +
                        $"created at [{existingStatSet.CreatedAt}]. Do you want to update it?",
                        yesText: "Yes",
                        noText: "No - Add New",
                        cancelText: null,
                        options: new DialogOptions { CloseOnEscapeKey = true });

                    if (dialogResult == null)
                    {
                        await StatSetService.ClearStagedChanges();
                        return Result<StatSet?>.Failure("User cancelled the operation.");
                    }
                    else if (dialogResult == true)
                    {
                        // Stage update to existing stat set
                        existingStatSet.Hands = Form.Hands ?? -1;
                        existingStatSet.Vpip = Form.Vpip ?? -1;
                        existingStatSet.Pfr = Form.Pfr ?? -1;
                        existingStatSet.Steal = Form.Steal ?? -1;
                        existingStatSet.ThreeBet = Form.ThreeBet ?? -1;

                        statSetResult = await StatSetService.StageAsync(existingStatSet, StageAction.Update);

                        // If stat set was staged successfully
                        if (statSetResult.IsSuccess)
                        {
                            Form.SelectedStatSet = statSetResult.Value!;
                        }

                        return statSetResult;
                    }
                    else if (dialogResult == false)
                    {
                        // Fall through to 'Stage new stat set'
                    }
                }
            }

            // Stage new stat set
            statSetResult = await StatSetService.StageAsync(new StatSet
            {
                PlayerId = Form.SelectedPlayer!.Id,
                TableId = Form.SelectedTable!.Id,
                Hands = Form.Hands ?? -1,
                Vpip = Form.Vpip ?? -1,
                Pfr = Form.Pfr ?? -1,
                Steal = Form.Steal ?? -1,
                ThreeBet = Form.ThreeBet ?? -1
            }, StageAction.Add);

            // If stat set was staged unsuccessfully
            if (statSetResult.IsFailure)
            {
                await StatSetService.ClearStagedChanges();
                return statSetResult;
            }

            // Stat set was staged successfully
            Form.SelectedStatSet = statSetResult.Value!;
            return statSetResult;
        }

        private async Task HandleValidSubmit()
        {
            // Stage new stat set
            await StatSetService.CommitAsync();

            // Build player note
            Form.PlayerNote = Form.SelectedPlayer!.ToString();

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

        private async Task<IEnumerable<string>> SearchNamesAsync(string value, CancellationToken cancellationToken)
        {
            var result = await PlayerService.GetAllAsync(p => EF.Functions.Like(p.Name, $"%{value}%"));
            return result.Value!.Select(p => p.Name);
        }

        private async Task OnSelectedNameChangedAsync(string? value)
        {
            Form.SelectedName = value;
            EditForm!.EditContext!.NotifyFieldChanged(EditForm.EditContext.Field("SelectedName"));
            await Task.CompletedTask;
        }

        private async Task HandleBlurAsync(FocusEventArgs args)
        {
            await OnSelectedNameChangedAsync(PlayerAutocomplete!.Text);
        }

        private async Task HandleKeyDownAsync(KeyboardEventArgs args)
        {
            // Default Autocomplete behaviour - select highlighted name
            if (args.Key == "Enter") return;

            // Select the current text whether the player exists or not
            if (args.Key == "Tab")
            {
                await OnSelectedNameChangedAsync(PlayerAutocomplete!.Text);
                await CloseAutocompleteMenuAsync();
            }
            // Close the Autocomplete control
            else if (args.Key == "Escape")
            {
                await CloseAutocompleteMenuAsync();
            }
        }

        private async Task CloseAutocompleteMenuAsync()
        {
            await PlayerAutocomplete!.CloseMenuAsync();
            await Task.Delay(100); // Give the dropdown time to close for Blur/FocusAsync to work
        }

        private async Task CopyPlayerNote() 
            => await JS.InvokeVoidAsync("navigator.clipboard.writeText", Form.PlayerNote);

        private async Task EnableForm() 
        {
            await PlayerAutocomplete!.ClearAsync();
            Form.Clear();

            _isFormDisabled = false;

            await TableNumericField!.FocusAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && PlayerAutocomplete is not null)
            {
                await TableNumericField!.FocusAsync();
            }
        }
    }
}
