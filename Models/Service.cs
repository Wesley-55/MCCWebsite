using System.ComponentModel.DataAnnotations;

namespace MCCConstruction.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string Icon { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }
    }
}