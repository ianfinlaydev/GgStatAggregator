using System.ComponentModel.DataAnnotations;

namespace GgStatAggregator.Models
{
    public class Table
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Stake Stake { get; set; }

        [Required]
        public int TableNumber { get; set; }
    }

    public enum Stake
    {
        None,
        NL2,
        NL5,
        NL10,
        NL25,
        NL50,
        NL100,
    }
}
