using System.ComponentModel.DataAnnotations;

namespace GgStatAggregator.Models
{
    public class Table
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Stake Stake { get; set; }

        [Required]
        public int TableNumber { get; set; }

        public override string ToString() => $"{Stake}({TableNumber})";
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
