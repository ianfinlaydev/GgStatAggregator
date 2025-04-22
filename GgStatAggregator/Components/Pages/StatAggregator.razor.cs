using GgStatAggregator.Components.Dialogs;
using GgStatAggregator.Models;
using GgStatAggregator.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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
        IJSRuntime js) : ComponentBase
    {
        private readonly IPlayerService PlayerService = playerService;
        private readonly IStatSetService StatSetService = statSetService;
        private readonly ITableService TableService = tableService;
        private readonly IDialogService DialogService = dialogService;
        private readonly IJSRuntime JS = js;

        private readonly IEnumerable<Stake> AllStakes = Enum.GetValues<Stake>().Cast<Stake>();
        private IEnumerable<Player> AllPlayers;
        private IEnumerable<Table> AllTables;

        private MudAutocomplete<Player> PlayerAutocomplete;

        private StatAggregatorForm Model { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            AllPlayers = await PlayerService.GetAllAsync();
            AllTables = await TableService.GetAllAsync();
        }

        private Task<IEnumerable<Player>> SearchPlayers(string value, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.FromResult(AllPlayers);
            }

            return Task.FromResult(AllPlayers
                .Where(p => p.Name.Contains(value, StringComparison.OrdinalIgnoreCase)));
        }

        private async Task OnValueChanged(Player player)
        {
            Model.SelectedPlayer = player;
            await Task.Delay(100); // Give the dropdown time to close for Blur to work
            await PlayerAutocomplete.BlurAsync();
        }

        private async Task OnValidSubmit(EditContext editContext)
        {
            var table = AllTables.FirstOrDefault(t => t.Stake == Model.SelectedStake && t.TableNumber == Model.SelectedTableNumber);

            if (table == null)
            {
                table = new Table()
                {
                    Stake = Model.SelectedStake,
                    TableNumber = (int)Model.SelectedTableNumber
                };

                await TableService.AddAsync(table);
            }

            await StatSetService.AddAsync(new StatSet()
            {
                PlayerId = Model.SelectedPlayer.Id,
                TableId = table.Id,
                Hands = Model.Hands,
                Vpip = Model.Vpip,
                Pfr = Model.Pfr,
                Steal = Model.Steal,
                ThreeBet = Model.ThreeBet
            });

            Model.SelectedPlayer = await PlayerService.GetByIdAsync(Model.SelectedPlayer.Id);
            Model.PlayerNote = Model.SelectedPlayer.ToString();

            StateHasChanged();
        }

        private async Task AddNewPlayerAsync()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };

            var dialog = await DialogService.ShowAsync<AddPlayerDialog>("Add New Player", options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var name = result.Data.ToString();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    await PlayerService.AddAsync(new Player 
                    { 
                        Name = name 
                    });

                    await DialogService.ShowMessageBox("Success", $"Player: '{name}' added successfully.", yesText: "OK");
                }

                AllPlayers = await PlayerService.GetAllAsync();

                StateHasChanged();
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
        public Player SelectedPlayer { get; set; }

        [Required(ErrorMessage = "Stake must be selected.")]
        [Range(1, int.MaxValue, ErrorMessage = "Stake must be selected.")]
        public Stake SelectedStake { get; set; }

        [Required(ErrorMessage = "Table number must be selected.")]
        public int? SelectedTableNumber { get; set; }

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

        public string PlayerNote { get; set; }
    }
}
