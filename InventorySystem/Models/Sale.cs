using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }

        // Auditoría: qué usuario realizó esta venta
        public int? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public User? CreatedBy { get; set; }

        public string PaymentMethod { get; set; } = "Efectivo";
        public decimal AmountPaid { get; set; }
        public decimal ChangeDue { get; set; }
        public string Currency { get; set; } = "COP";

        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
