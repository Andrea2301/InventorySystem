using InventorySystem.Data;
using InventorySystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface ISeedDataService
    {
        Task SeedAsync();
    }

    public class SeedDataService : ISeedDataService
    {
        private readonly AppDbContext _db;

        public SeedDataService(AppDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            if (_db.Products.Any())
            {
                throw new InvalidOperationException("Database already contains products. Delete existing data first or use the Refresh button to reload the dashboard.");
            }

            // 1. Categories and Products
            var products = new List<Product>
            {
                new Product { Name = "Laptop Pro 16", Category = "Electronics", Price = 1200.00m, Quantity = 15, Description = "High performance laptop" },
                new Product { Name = "Wireless Mouse", Category = "Accessories", Price = 25.50m, Quantity = 3, Description = "Ergonomic mouse" },
                new Product { Name = "Monitor 4K", Category = "Electronics", Price = 450.00m, Quantity = 8, Description = "32 inch 4K Display" },
                new Product { Name = "Mechanical Keyboard", Category = "Accessories", Price = 89.99m, Quantity = 20, Description = "RGB Backlit" },
                new Product { Name = "USB-C Hub", Category = "Accessories", Price = 45.00m, Quantity = 1, Description = "7-in-1 adapter" }
            };
            await _db.Products.AddRangeAsync(products);

            // 2. Clients
            var clients = new List<Client>
            {
                new Client { FirstName = "Juan", LastName = "Pérez", Email = "juan@example.com", DocumentNumber = "12345678", PhoneNumber = "555-0101" },
                new Client { FirstName = "María", LastName = "García", Email = "maria@example.com", DocumentNumber = "87654321", PhoneNumber = "555-0102" }
            };
            await _db.Clients.AddRangeAsync(clients);
            await _db.SaveChangesAsync(); // Save to get IDs

            // 3. Sales (Historical)
            var today = DateTime.Today;
            var random = new Random();
            var sales = new List<Sale>();

            for (int i = 5; i >= 0; i--)
            {
                var saleDate = today.AddMonths(-i);
                var sale = new Sale
                {
                    SaleDate = saleDate,
                    ClientId = clients[random.Next(clients.Count)].Id,
                    TotalAmount = random.Next(500, 3000),
                    SaleDetails = new List<SaleDetail>()
                };
                
                // Add some random details
                for (int j = 0; j < 3; j++)
                {
                    var p = products[random.Next(products.Count)];
                    int qty = random.Next(1, 4);
                    sale.SaleDetails.Add(new SaleDetail
                    {
                        ProductId = p.Id,
                        Quantity = qty,
                        UnitPrice = p.Price,
                        TotalPrice = qty * p.Price
                    });
                }
                sales.Add(sale);
            }
            await _db.Sales.AddRangeAsync(sales);
            await _db.SaveChangesAsync();
        }
    }
}
