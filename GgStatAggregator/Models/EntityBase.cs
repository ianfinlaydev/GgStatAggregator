using System.ComponentModel.DataAnnotations;

namespace GgStatAggregator.Models
{
    public abstract class EntityBase
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime? UpdatedAt { get; set; }
    }
}
