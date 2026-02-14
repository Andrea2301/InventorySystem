using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models
{
    public class Supplier : Person
    {
        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Website { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
