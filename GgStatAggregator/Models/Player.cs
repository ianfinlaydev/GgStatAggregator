using System.ComponentModel.DataAnnotations;

namespace GgStatAggregator.Models
{
    public class Player
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<StatSet> StatSets { get; set; } = [];

        public override string ToString() => $"Hands: {HandsAggregate}\n" +
           $"VPIP: {VpipAggregate:F1}\n" +
           $"PFR: {PfrAggregate:F1}\n" +
           $"ATS: {StealAggregate:F1}\n" +
           $"3Bet: {ThreeBetAggregate:F1}";

        #region Aggregate Properties
        public int HandsAggregate 
        { 
            get => StatSets.Sum(s => s.Hands); 
        }

        public double VpipAggregate
        {
            get
            {
                if (HandsAggregate == 0) return 0;
                return StatSets.Sum(s => s.Vpip * s.Hands) / HandsAggregate;
            }
        }

        public double PfrAggregate
        {
            get
            {
                if (HandsAggregate == 0) return 0;
                return StatSets.Sum(s => s.Pfr * s.Hands) / HandsAggregate;
            }
        }

        public double StealAggregate
        {
            get
            {
                if (HandsAggregate == 0) return 0;
                return StatSets.Sum(s => s.Steal * s.Hands) / HandsAggregate;
            }
        }

        public double ThreeBetAggregate 
        {
            get
            {
                if (HandsAggregate == 0) return 0;
                return StatSets.Sum(s => s.ThreeBet * s.Hands) / HandsAggregate;
            } 
        }
        #endregion Aggregate Properties
    }
}
