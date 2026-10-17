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
                return;
            }

            // 1. Products matching the Cloud Sister App
            var laptop = new Product
            {
                Name = "Laptop Pro 13",
                Category = "General",
                Price = 1333m,
                Quantity = 1, // Triggers Stock Alert = 1
                Description = "Portátil ultraligero de alto rendimiento",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var monitor = new Product
            {
                Name = "Monitor Curvo 34",
                Category = "Electronics",
                Price = 14297m,
                Quantity = 1, // 1333 + 14297 = COP 15,630 total inventory value
                Description = "Monitor profesional panorámico 4K",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var products = new List<Product> { laptop, monitor };
            await _db.Products.AddRangeAsync(products);

            // 2. Client matching Cloud Sister App
            var clientDairo = new Client
            {
                FirstName = "Dairo",
                LastName = "ospino",
                Email = "dairo.ospino@example.com",
                DocumentNumber = "1045238910",
                PhoneNumber = "+57 300 123 4567",
                Address = "Calle 100 # 15-20",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Clients.AddAsync(clientDairo);
            await _db.SaveChangesAsync(); // Save to obtain generated IDs

            // 3. Recent Sale matching Cloud Sister App
            var demoSale = new Sale
            {
                ClientId = clientDairo.Id,
                SaleDate = DateTime.Now,
                TotalAmount = 1333m,
                PaymentMethod = "Cash",
                Currency = "COP",
                AmountPaid = 1333m,
                ChangeDue = 0m,
                SaleDetails = new List<SaleDetail>
                {
                    new SaleDetail
                    {
                        ProductId = laptop.Id,
                        Quantity = 1,
                        UnitPrice = 1333m,
                        TotalPrice = 1333m
                    }
                }
            };

            await _db.Sales.AddAsync(demoSale);
            await _db.SaveChangesAsync();
        }
    }
}
