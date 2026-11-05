using System.ComponentModel.DataAnnotations;

namespace MCCConstruction.Models
{
    public class QuoteRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(300)]
        public string ProjectAddress { get; set; } = string.Empty;

        [StringLength(100)]
        public string ServiceRequired { get; set; } = string.Empty;

        [Range(0, 999999999)]
        public decimal? Budget { get; set; }

        public DateTime? ExpectedStartDate { get; set; }

        public string Description { get; set; } = string.Empty;

        // Stores the private Supabase Storage object path.
        public string AttachmentPath { get; set; } = string.Empty;

        // Store submission timestamps in UTC.
        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "New";
    }
}