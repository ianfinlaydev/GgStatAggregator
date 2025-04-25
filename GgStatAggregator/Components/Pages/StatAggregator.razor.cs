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
    public partial class StatAggregator(IPlayerService playerService, 
        IStatSetService statSetService, 
        ITableService tableService, 
        IDialogService dialogService,
        IJSRuntime js,
        ILogger<StatAggregator> logger) : ComponentBase
    {
        private readonly IPlayerService PlayerService = playerService;
        private readonly IStatSetService StatSetService = statSetService;
        private readonly ITableService TableService = tableService;
        private readonly IDialogService DialogService = dialogService;
        private readonly IJSRuntime JS = js;
        private readonly ILogger<StatAggregator> Logger = logger;

        private readonly IEnumerable<Stake> Stakes = Enum.GetValues<Stake>().Cast<Stake>();

        private StatAggregatorForm Model = new();
        private MudAutocomplete<string> PlayerAutocomplete;
        private MudNumericField<int> HandNumericField;
        private MudNumericField<int> TableNumericField;
        private EditForm editForm;

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
            {
                return;
            }

            Model.SelectedTable = (await TableService
                .GetAllAsync(t => t.Stake == Model.SelectedStake && t.TableNumber == Model.SelectedTableNumber)).FirstOrDefault();

            Model.SelectedPlayer = (await PlayerService
                .GetAllAsync(p => p.Name == Model.SelectedName)).FirstOrDefault();

            if (Model.SelectedTable == null)
            {
                Model.SelectedTable = new Table()
                {
                    Stake = Model.SelectedStake,
                    TableNumber = Model.SelectedTableNumber
                };

                await TableService.AddAsync(Model.SelectedTable);
            }

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

            await StatSetService.AddAsync(Model.StatSet);

            Model.SelectedPlayer.StatSets = await StatSetService.GetAllAsync(s => s.PlayerId == Model.SelectedPlayer.Id);
            Model.PlayerNote = Model.SelectedPlayer.ToString();

            StateHasChanged();

            //TODO: default exit on escape key
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
                await HandNumericField.FocusAsync();
            }
            else if (args.Key == "Escape")
            {
                await PlayerAutocomplete.CloseMenuAsync();
                await Task.Delay(100); // Give the dropdown time to close for Blur to work
                await PlayerAutocomplete.BlurAsync();
            }
        }

        private async Task CopyPlayerNote() => await JS.InvokeVoidAsync("navigator.clipboard.writeText", Model.PlayerNote);

        private async Task ClearPlayerFields() 
        {
            Model.SelectedPlayer = null;
            Model.PlayerNote = string.Empty;
            Model.Hands = 0;
            Model.Vpip = 0;
            Model.Pfr = 0;
            Model.Steal = 0;
            Model.ThreeBet = 0;
            await PlayerAutocomplete.ClearAsync();
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
