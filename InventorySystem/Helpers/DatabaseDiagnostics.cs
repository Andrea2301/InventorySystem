using InventorySystem.Data;
using System;
using System.Linq;
using System.Windows;

namespace InventorySystem.Helpers
{
    public static class DatabaseDiagnostics
    {
        public static void ShowDatabaseStats(AppDbContext db)
        {
            try
            {
                int productCount = db.Products.Count();
                int clientCount = db.Clients.Count();
                int saleCount = db.Sales.Count();
                int saleDetailCount = db.SaleDetails.Count();
                int supplierCount = db.Suppliers.Count();

                string message = $"Database Statistics:\n\n" +
                                $"Products: {productCount}\n" +
                                $"Clients: {clientCount}\n" +
                                $"Sales: {saleCount}\n" +
                                $"Sale Details: {saleDetailCount}\n" +
                                $"Suppliers: {supplierCount}";

                MessageBox.Show(message, "Database Diagnostics", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
