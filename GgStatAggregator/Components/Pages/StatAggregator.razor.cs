using GgStatAggregator.Components.Dialogs;
using GgStatAggregator.Models;
using GgStatAggregator.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using static MudBlazor.Colors;

namespace GgStatAggregator.Components.Pages
{
    public partial class StatAggregator(IService<Player> playerService, 
        IService<StatSet> statSetService, 
        IService<Table> tableService, 
        IDialogService dialogService,
        IJSRuntime js,
        ILogger<StatAggregator> logger) : ComponentBase
    {
        private readonly IService<Player> PlayerService = playerService;
        private readonly IService<StatSet> StatSetService = statSetService;
        private readonly IService<Table> TableService = tableService;
        private readonly IDialogService DialogService = dialogService;
        private readonly IJSRuntime JS = js;
        private readonly ILogger<StatAggregator> Logger = logger;

        private readonly IEnumerable<Stake> Stakes = Enum.GetValues<Stake>().Cast<Stake>();

        private StatAggregatorForm Model = new();
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
            IEnumerable<Player> players = await PlayerService.GetAllAsync(p => EF.Functions.Like(p.Name, $"%{value}%"));

            return players.Select(p => p.Name);
        }

        private async Task BeforeSubmit()
        {
            if (!editForm.EditContext.Validate())
                return;

            // Try to find existing table
            Model.SelectedTable = await TableService.GetFirstOrDefaultAsync(
                t => t.Stake == Model.SelectedStake && t.TableNumber == Model.SelectedTableNumber);

            // If no table found, create and save a new one
            if (Model.SelectedTable == null)
            {
                Model.SelectedTable = new Table()
                {
                    Stake = Model.SelectedStake,
                    TableNumber = Model.SelectedTableNumber
                };

                Model.SelectedTable = await TableService.AddAsync(Model.SelectedTable);
            }

            // Try to find existing player
            Model.SelectedPlayer = await PlayerService.GetFirstOrDefaultAsync(p => p.Name == Model.SelectedName);

            // If no player found, ask user if they want to create one
            if (Model.SelectedPlayer == null)
            {
                var result = await DialogService.ShowMessageBox(
                    "Create Player",
                    $"Player: [{Model.SelectedName}] does not exist. Create new player?",
                    yesText: "Yes",
                    cancelText: "Cancel",
                    options: new DialogOptions { CloseOnEscapeKey = true });

                if (result == false)
                    return;

                Model.SelectedPlayer = new Player()
                {
                    Name = Model.SelectedName
                };

                await PlayerService.AddAsync(Model.SelectedPlayer);
            }

            // Only proceed if both entities are ready
            if (Model.SelectedTable is not null 
                && Model.SelectedPlayer is not null)
            {
                await HandleValidSubmit(editForm.EditContext);
            }
        }

        private async Task HandleValidSubmit(EditContext editContext)
        {
            Model.StatSet = new StatSet()
            {
                PlayerId = Model.SelectedPlayer.Id,
                TableId = Model.SelectedTable.Id,
                Hands = Model.Hands,
                Vpip = Model.Vpip,
                Pfr = Model.Pfr,
                Steal = Model.Steal,
                ThreeBet = Model.ThreeBet
            };

            // Save the new StatSet
            Model.StatSet = await StatSetService.AddAsync(Model.StatSet);

            // Refresh the player's stat sets
            Model.SelectedPlayer.StatSets = await StatSetService.GetAllAsync(
                s => s.PlayerId == Model.SelectedPlayer.Id
            );

            // Build player note
            Model.PlayerNote = Model.SelectedPlayer.ToString();

            // Disable the form after submission
            _isFormDisabled = true;

            // Refresh the UI
            StateHasChanged();

            // Show success message
            await DialogService.ShowMessageBox("Success", 
                $"Stats for {Model.SelectedPlayer.Name} added successfully.", 
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
                Model.SelectedName = PlayerAutocomplete.Text;
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
            => await JS.InvokeVoidAsync("navigator.clipboard.writeText", Model.PlayerNote);

        private void EnableForm() 
        {
            Model = new StatAggregatorForm
            {
                SelectedStake = Model.SelectedStake,
                SelectedTableNumber = Model.SelectedTableNumber
            };

            _isFormDisabled = false;
        }
    }

    public class StatAggregatorForm
    {
        [Required(ErrorMessage = "Player must be selected.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Player name must be 1-50 character.")]
        public string SelectedName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Stake must be selected.")]
        public Stake SelectedStake { get; set; } = Stake.NL10;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
        public int SelectedTableNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
        public int Hands { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Value must be between 0 and 100.")]
        public double Vpip { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Value must be between 0 and 100.")]
        public double Pfr { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Value must be between 0 and 100.")]
        public double Steal { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Value must be between 0 and 100.")]
        public double ThreeBet { get; set; }

        public Player SelectedPlayer { get; set; }

        public Table SelectedTable { get; set; }

        public StatSet StatSet { get; set; }

        public string PlayerNote { get; set; }
    }
}
