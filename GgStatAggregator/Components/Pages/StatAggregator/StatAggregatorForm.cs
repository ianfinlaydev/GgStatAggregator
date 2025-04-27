using GgStatAggregator.Models;
using GgStatAggregator.Services;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace GgStatAggregator.Components.Pages.StatAggregator
{
    public class StatAggregatorForm
    {
        [Required(ErrorMessage = "Player name must be selected.")]
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

        [Required]
        public Player SelectedPlayer { get; set; }

        [Required]
        public Table SelectedTable { get; set; }

        public StatSet StatSet { get; set; }

        public string PlayerNote { get; set; }

        public void Clear()
        {
            SelectedName = string.Empty;
            Hands = 0;
            Vpip = 0;
            Pfr = 0;
            Steal = 0;
            ThreeBet = 0;
            PlayerNote = string.Empty;
            SelectedPlayer = null;
            SelectedTable = null;
            StatSet = null;
        }
    }
}
