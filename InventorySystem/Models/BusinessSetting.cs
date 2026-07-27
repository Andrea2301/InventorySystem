using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models
{
    public class BusinessSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = "INVENTORY SYSTEM";

        [MaxLength(100)]
        public string TaxId { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal TaxPercentage { get; set; } = 0.00m;

        [Required]
        [MaxLength(5)]
        public string CurrencySymbol { get; set; } = "$";
    }
}
