using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Cashier;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        // Navigation
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        [NotMapped]
        public string RoleDisplay => Role == UserRole.Admin ? "Administrator" : "Cashier";
    }
}
