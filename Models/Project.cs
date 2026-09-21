using System.ComponentModel.DataAnnotations;

namespace MCCConstruction.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Client { get; set; } = string.Empty;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public DateTime? CompletionDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public bool Featured { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}